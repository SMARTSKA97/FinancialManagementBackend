using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Accounts;

namespace FinancialPlanner.Application.Contracts;

public interface IAccountService
{
    Task<Result<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams);
    Task<Result<AccountDto>> UpsertAccountAsync(string userId, UpsertAccountDto dto);
    Task<Result<bool>> DeleteAccountAsync(string userId, int accountId);
}