using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Contracts;

// This interface now lives in the Application layer, where it belongs.
public interface IUnitOfWork : IAsyncDisposable
{
    IAccountRepository AccountRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    IGenericRepository<AccountCategory> AccountCategoryRepository { get; }
    IGenericRepository<TransactionCategory> TransactionCategoryRepository { get; }

    IGenericRepository<Feedback> FeedbackRepository { get; }

    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> CompleteAsync(); // This is the only method that saves changes
}