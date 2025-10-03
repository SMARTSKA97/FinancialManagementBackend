using FinancialPlanner.Application;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<Account?> GetByIdAsync(int id, bool includeRelated = false)
    {
        if (includeRelated)
        {
            return await _context.Accounts
                .Include(a => a.AccountCategory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        return await base.GetByIdAsync(id);
    }

    public async Task<PaginatedResult<Account>> GetPagedAccountsAsync(string userId, QueryParameters queryParams)
    {
        var queryable = _context.Accounts
            .Where(a => a.UserId == userId)
            .Include(a => a.AccountCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            queryable = queryable.Where(a =>
                a.Name.Contains(queryParams.GlobalSearch) ||
                (a.AccountCategory != null && a.AccountCategory.Name.Contains(queryParams.GlobalSearch)));
        }

        var totalRecords = await queryable.CountAsync();

        var items = await queryable
            .OrderBy(a => a.Name)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PaginatedResult<Account>(items, totalRecords, queryParams.PageNumber, queryParams.PageSize);
    }
}