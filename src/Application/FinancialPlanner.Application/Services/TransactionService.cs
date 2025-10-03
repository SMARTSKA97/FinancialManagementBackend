using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<PaginatedResult<TransactionDto>>.Failure("Account not found.");

        var pagedData = await _transactionRepository.GetPagedTransactionsForAccountAsync(accountId, queryParams);
        var pagedDto = new PaginatedResult<TransactionDto>(
            _mapper.Map<List<TransactionDto>>(pagedData.Data),
            pagedData.TotalRecords,
            pagedData.PageNumber,
            pagedData.PageSize
        );
        return ApiResponse<PaginatedResult<TransactionDto>>.Success(pagedDto);
    }

    public async Task<ApiResponse<TransactionDto>> UpsertTransactionAsync(string userId, int accountId, UpsertTransactionDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<TransactionDto>.Failure("Account not found.");

        Transaction transaction;
        decimal oldAmount = 0;
        TransactionType oldType = dto.Type; // Default to the new type

        if (dto.Id.HasValue && dto.Id.Value > 0)
        {
            transaction = await _transactionRepository.GetByIdAsync(dto.Id.Value, true);
            if (transaction == null || transaction.AccountId != accountId)
                return ApiResponse<TransactionDto>.Failure("Transaction not found.");

            oldAmount = transaction.Amount;
            oldType = transaction.Type;
            _mapper.Map(dto, transaction);
        }
        else
        {
            transaction = _mapper.Map<Transaction>(dto);
            transaction.AccountId = accountId;
        }

        account.Balance += (oldType == TransactionType.Income ? -oldAmount : oldAmount);
        account.Balance += (transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount);

        await _accountRepository.UpsertAsync(account);
        var savedTransaction = await _transactionRepository.UpsertAsync(transaction);
        var resultDto = _mapper.Map<TransactionDto>(savedTransaction);

        return ApiResponse<TransactionDto>.Success(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteTransactionAsync(string userId, int accountId, int transactionId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<bool>.Failure("Account not found.");

        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.AccountId != accountId)
            return ApiResponse<bool>.Failure("Transaction not found.");

        account.Balance += (transaction.Type == TransactionType.Income ? -transaction.Amount : transaction.Amount);
        await _accountRepository.UpsertAsync(account);
        await _transactionRepository.DeleteAsync(transaction);

        return ApiResponse<bool>.Success(true, "Transaction deleted successfully.");
    }
}