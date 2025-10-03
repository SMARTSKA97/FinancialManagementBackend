using FinancialPlanner.Application.DTOs.Accounts;

namespace FinancialPlanner.Application.Contracts;

public interface IAccountService
{
    Task<ApiResponse<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams);
    Task<ApiResponse<AccountDto>> UpsertAccountAsync(string userId, UpsertAccountDto dto);
    Task<ApiResponse<bool>> DeleteAccountAsync(string userId, int accountId);
}