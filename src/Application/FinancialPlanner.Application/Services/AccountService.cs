using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Accounts;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork; // Use Unit of Work
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<AccountDto>>> GetPagedAccountsAsync(string userId, QueryParameters queryParams)
    {
        var pagedData = await _unitOfWork.AccountRepository.GetPagedAccountsAsync(userId, queryParams);
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
            account = (await _unitOfWork.AccountRepository.GetByIdAsync(dto.Id.Value, true))!;
            if (account == null || account.UserId != userId)
                return ApiResponse<AccountDto>.Failure("Account not found.");
            _mapper.Map(dto, account);
            _unitOfWork.AccountRepository.Upsert(account); // Stage the update
        }
        else
        {
            account = _mapper.Map<Account>(dto);
            account.UserId = userId;
            _unitOfWork.AccountRepository.Upsert(account); // Stage the create
        }

        await _unitOfWork.CompleteAsync(); // Save all changes

        var resultDto = _mapper.Map<AccountDto>(account);
        return ApiResponse<AccountDto>.Success(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteAccountAsync(string userId, int accountId)
    {
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<bool>.Failure("Account not found.");

        _unitOfWork.AccountRepository.Delete(account); // Stage the delete
        await _unitOfWork.CompleteAsync(); // Save all changes

        return ApiResponse<bool>.Success(true, "Account deleted successfully.");
    }
}