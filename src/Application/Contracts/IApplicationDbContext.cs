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
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
