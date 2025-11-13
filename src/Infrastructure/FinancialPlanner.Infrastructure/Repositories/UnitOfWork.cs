using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinancialPlanner.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories are now lazy-loaded
    private IAccountRepository? _accountRepository;
    private ITransactionRepository? _transactionRepository;
    private IGenericRepository<AccountCategory>? _accountCategoryRepository;
    private IGenericRepository<TransactionCategory>? _transactionCategoryRepository;
    private IGenericRepository<Feedback>? _feedbackRepository; // Add private field

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IAccountRepository AccountRepository => _accountRepository ??= new AccountRepository(_context);
    public ITransactionRepository TransactionRepository => _transactionRepository ??= new TransactionRepository(_context);
    public IGenericRepository<AccountCategory> AccountCategoryRepository => _accountCategoryRepository ??= new GenericRepository<AccountCategory>(_context);
    public IGenericRepository<TransactionCategory> TransactionCategoryRepository => _transactionCategoryRepository ??= new GenericRepository<TransactionCategory>(_context);

    // --- THIS IS THE FIX ---
    public IGenericRepository<Feedback> FeedbackRepository => _feedbackRepository ??= new GenericRepository<Feedback>(_context); // Add public implementation

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
    }
}