using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace FinancialPlanner.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // --- DbSets for Main Entities ---
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountCategory> AccountCategories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionCategory> TransactionCategories { get; set; }

    // --- DbSets for Log Entities ---
    public DbSet<AccountLog> AccountLogs { get; set; }
    public DbSet<AccountCategoryLog> AccountCategoryLogs { get; set; }
    public DbSet<TransactionLog> TransactionLogs { get; set; }
    public DbSet<TransactionCategoryLog> TransactionCategoryLogs { get; set; }


    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseEntity>().ToList();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Metadata.GetTableName()!,
                UserId = "system" // Placeholder; would use IHttpContextAccessor in a real app
            };

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
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
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
                            entry.Entity.UpdatedAt = DateTime.UtcNow;
                            auditEntry.Action = "UPDATE";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                        }
                        break;
                }
            }
            auditEntries.Add(auditEntry);
        }
        return auditEntries;
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            int entityId = Convert.ToInt32(auditEntry.KeyValues["Id"]);

            // Use a switch to create the correct log type
            AuditLog log = auditEntry.TableName switch
            {
                "Accounts" => new AccountLog(),
                "AccountCategories" => new AccountCategoryLog(),
                "Transactions" => new TransactionLog(),
                "TransactionCategories" => new TransactionCategoryLog(),
                _ => throw new NotSupportedException($"Audit logging for table {auditEntry.TableName} is not supported.")
            };

            log.EntityId = entityId;
            log.Action = auditEntry.Action;
            log.OldData = auditEntry.OldValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.OldValues);
            log.NewData = auditEntry.NewValues.Count == 0 ? null : JsonSerializer.Serialize(auditEntry.NewValues);
            log.ChangedByUserId = auditEntry.UserId;
            log.ChangedAt = DateTime.UtcNow;

            this.Add(log); // Add the generic log object to the context
        }
        return SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- IDENTITY SCHEMA ---
        builder.Entity<ApplicationUser>(e => e.ToTable("Users", "identity"));
        builder.Entity<IdentityRole>(e => e.ToTable("Roles", "identity"));
        builder.Entity<IdentityUserRole<string>>(e => e.ToTable("UserRoles", "identity"));
        builder.Entity<IdentityUserClaim<string>>(e => e.ToTable("UserClaims", "identity"));
        builder.Entity<IdentityUserLogin<string>>(e => e.ToTable("UserLogins", "identity"));
        builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable("RoleClaims", "identity"));
        builder.Entity<IdentityUserToken<string>>(e => e.ToTable("UserTokens", "identity"));


        // --- ACCOUNTS SCHEMA ---
        builder.Entity<Account>(e => e.ToTable("Accounts", "accounts"));
        builder.Entity<AccountCategory>(e => e.ToTable("AccountCategories", "accounts"));
        builder.Entity<AccountLog>(e => e.ToTable("AccountLogs", "accounts"));
        builder.Entity<AccountCategoryLog>(e => e.ToTable("AccountCategoryLogs", "accounts"));

        // --- TRANSACTIONS SCHEMA ---
        builder.Entity<Transaction>(e => e.ToTable("Transactions", "transactions"));
        builder.Entity<TransactionCategory>(e => e.ToTable("TransactionCategories", "transactions"));
        builder.Entity<TransactionLog>(e => e.ToTable("TransactionLogs", "transactions"));
        builder.Entity<TransactionCategoryLog>(e => e.ToTable("TransactionCategoryLogs", "transactions"));
    }
}


