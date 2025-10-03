using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Contracts;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<PaginatedResult<Transaction>> GetPagedTransactionsForAccountAsync(int accountId, QueryParameters queryParams);
}