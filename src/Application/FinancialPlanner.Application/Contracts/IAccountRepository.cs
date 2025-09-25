using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Contracts;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<IReadOnlyList<Account>> GetAccountsByUserIdAsync(string userId);
}