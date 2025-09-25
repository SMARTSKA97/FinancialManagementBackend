using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.Helpers;
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

    public async Task<PaginatedResult<Transaction>> GetPagedTransactionsForAccountAsync(int accountId, QueryParameters queryParams)
    {
        var queryable = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .Include(t => t.Category) // Include category data for the response
            .AsQueryable();

        // Apply Global Search Filter
        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            queryable = queryable.Where(t => t.Description.Contains(queryParams.GlobalSearch));
        }

        // TODO: Apply advanced sorting logic here based on queryParams.SortBy

        var totalRecords = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(t => t.Date) // Default sort
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PaginatedResult<Transaction>(items, totalRecords, queryParams.PageNumber, queryParams.PageSize);
    }
}