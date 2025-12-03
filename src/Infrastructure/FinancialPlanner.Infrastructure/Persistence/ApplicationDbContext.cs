using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;

namespace FinancialPlanner.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // --- DbSets for Main Entities ---
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountCategory> AccountCategories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionCategory> TransactionCategories { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    // --- DbSets for Log Entities ---
    public DbSet<AccountLog> AccountLogs { get; set; }
    public DbSet<AccountCategoryLog> AccountCategoryLogs { get; set; }
    public DbSet<TransactionLog> TransactionLogs { get; set; }
    public DbSet<TransactionCategoryLog> TransactionCategoryLogs { get; set; }
    public DbSet<FeedbackLog> FeedbackLogs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        SetAuditProperties();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private void SetAuditProperties()
    {
        // FIX: Track IAuditable instead of BaseEntity to catch ApplicationUser too
        var entries = ChangeTracker.Entries<IAuditable>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        // FIX: Use IAuditable here as well
        var entries = ChangeTracker.Entries<IAuditable>().ToList();
        var auditEntries = new List<AuditEntry>();
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry) { TableName = entry.Metadata.GetTableName()!, UserId = userId };

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary) continue;
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                    continue;
                }
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.Action = "CREATE";
                        auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        break;
                    case EntityState.Deleted:
                        auditEntry.Action = "DELETE";
                        auditEntry.OldValues[propertyName] = property.OriginalValue!;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.Action = "UPDATE";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
            if (!string.IsNullOrEmpty(auditEntry.Action))
            {
                auditEntries.Add(auditEntry);
            }
        }
        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            // FIX: Check if entity is supported for logging BEFORE accessing ID
            AuditLog? log = auditEntry.Entry.Entity switch
            {
                Account _ => new AccountLog(),
                AccountCategory _ => new AccountCategoryLog(),
                Transaction _ => new TransactionLog(),
                TransactionCategory _ => new TransactionCategoryLog(),
                Feedback _ => new FeedbackLog(),
                _ => null
            };

            if (log == null) continue;

            // Safe to access ID now
            int entityId = Convert.ToInt32(auditEntry.Entry.Property("Id").CurrentValue);
            var oldData = auditEntry.OldValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.OldValues);
            var newData = auditEntry.NewValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.NewValues);

            log.EntityId = entityId;
            log.Action = auditEntry.Action;
            log.OldData = oldData;
            log.NewData = newData;
            log.ChangedByUserId = auditEntry.UserId;
            log.ChangedAt = DateTime.UtcNow;

            await this.AddAsync(log);
        }
        // Save the log entries in a separate transaction
        await base.SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- IDENTITY SCHEMA ---
        builder.Entity<ApplicationUser>(e => {
            e.ToTable("Users", "identity");
            e.OwnsMany(p => p.RefreshTokens, a =>
            {
                a.ToTable("RefreshTokens", "identity");
                a.WithOwner().HasForeignKey("UserId");
            });
        });
        builder.Entity<IdentityRole>(e => e.ToTable("Roles", "identity"));
        builder.Entity<IdentityUserRole<string>>(e => e.ToTable("UserRoles", "identity"));
        builder.Entity<IdentityUserClaim<string>>(e => e.ToTable("UserClaims", "identity"));
        builder.Entity<IdentityUserLogin<string>>(e => e.ToTable("UserLogins", "identity"));
        builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable("RoleClaims", "identity"));
        builder.Entity<IdentityUserToken<string>>(e => e.ToTable("UserTokens", "identity"));

        // --- ACCOUNTS SCHEMA ---
        builder.Entity<Account>(e => e.ToTable("Accounts", "accounts"));
        builder.Entity<AccountCategory>(e =>
        {
            e.ToTable("AccountCategories", "accounts");
            e.HasIndex(ac => new { ac.UserId, ac.Name }).IsUnique();
        });
        builder.Entity<AccountLog>(e => e.ToTable("AccountLogs", "accounts"));
        builder.Entity<AccountCategoryLog>(e => e.ToTable("AccountCategoryLogs", "accounts"));

        // --- TRANSACTIONS SCHEMA ---
        builder.Entity<Transaction>(e => e.ToTable("Transactions", "transactions"));
        builder.Entity<TransactionCategory>(e =>
        {
            e.ToTable("TransactionCategories", "transactions");
            e.HasIndex(tc => new { tc.UserId, tc.Name }).IsUnique();
        });
        builder.Entity<TransactionLog>(e => e.ToTable("TransactionLogs", "transactions"));
        builder.Entity<TransactionCategoryLog>(e => e.ToTable("TransactionCategoryLogs", "transactions"));

        // --- FEEDBACK SCHEMA ---
        builder.Entity<Feedback>(e => e.ToTable("Feedbacks", "feedback"));
        builder.Entity<FeedbackLog>(e => e.ToTable("FeedbackLogs", "feedback"));
    }
}