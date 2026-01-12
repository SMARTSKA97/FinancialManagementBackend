using Application.Common.Models;
using Application.DTOs.Accounts;

namespace Application.Contracts;

public interface IAccountService
{
    Task<Result<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams);
    Task<Result<AccountDto>> UpsertAccountAsync(string userId, UpsertAccountDto dto);
    Task<Result<bool>> DeleteAccountAsync(string userId, int accountId);
}