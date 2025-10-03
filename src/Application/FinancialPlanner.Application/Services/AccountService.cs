using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Accounts;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams)
    {
        var pagedData = await _accountRepository.GetPagedAccountsAsync(userId, queryParams);

        var pagedDto = new PaginatedResult<AccountDto>(
            _mapper.Map<List<AccountDto>>(pagedData.Data),
            pagedData.TotalRecords,
            pagedData.PageNumber,
            pagedData.PageSize
        );

        return ApiResponse<PaginatedResult<AccountDto>>.Success(pagedDto);
    }

    public async Task<ApiResponse<AccountDto>> UpsertAccountAsync(string userId, UpsertAccountDto dto)
    {
        Account account;
        if (dto.Id.HasValue && dto.Id > 0)
        {
            account = await _accountRepository.GetByIdAsync(dto.Id.Value);
            if (account == null || account.UserId != userId)
            {
                return ApiResponse<AccountDto>.Failure("Account not found.");
            }
            _mapper.Map(dto, account);
        }
        else
        {
            account = _mapper.Map<Account>(dto);
            account.UserId = userId;
        }

        var savedAccount = await _accountRepository.UpsertAsync(account);
        var resultAccountWithCategory = await _accountRepository.GetByIdAsync(savedAccount.Id);
        var resultDto = _mapper.Map<AccountDto>(resultAccountWithCategory);
        return ApiResponse<AccountDto>.Success(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteAccountAsync(string userId, int accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
        {
            return ApiResponse<bool>.Failure("Account not found.");
        }

        await _accountRepository.DeleteAsync(account);
        return ApiResponse<bool>.Success(true, "Account deleted successfully.");
    }
}