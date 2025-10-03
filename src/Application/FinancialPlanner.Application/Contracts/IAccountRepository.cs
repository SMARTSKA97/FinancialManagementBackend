using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Contracts;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<PaginatedResult<Account>> GetPagedAccountsAsync(string userId, QueryParameters queryParams);
}