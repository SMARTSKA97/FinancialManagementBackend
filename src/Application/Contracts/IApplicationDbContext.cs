using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Contracts;

public interface IApplicationDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<AccountCategory> AccountCategories { get; }
    DbSet<TransactionCategory> TransactionCategories { get; }
    DbSet<Feedback> Feedbacks { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<RecurringTransaction> RecurringTransactions { get; }

    DbSet<Issue> Issues { get; }
    DbSet<IssueTaxonomy> IssueTaxonomies { get; }
    DbSet<IssueComment> IssueComments { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
