using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Repositories;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<Transaction?> GetByIdAsync(int id, bool includeRelated = false)
    {
        IQueryable<Transaction> query = _context.Transactions;
        if (includeRelated)
        {
            query = query.Include(t => t.TransactionCategory);
        }
        return await query.Include(t => t.TransactionCategory).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<PaginatedResult<Transaction>> GetPagedTransactionsForAccountAsync(int accountId, QueryParameters queryParams)
    {
        var queryable = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .Include(t => t.TransactionCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            queryable = queryable.Where(t => t.Description.Contains(queryParams.GlobalSearch) ||
                (t.TransactionCategory != null && t.TransactionCategory.Name.Contains(queryParams.GlobalSearch)));
        }

        var totalRecords = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(t => t.Date)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PaginatedResult<Transaction>(items, totalRecords, queryParams.PageNumber, queryParams.PageSize);
    }
}
