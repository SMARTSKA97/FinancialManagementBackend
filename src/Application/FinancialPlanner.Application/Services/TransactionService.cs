using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Application.Helpers;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public interface ITransactionService
{
    Task<ApiResponse<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams);
    Task<ApiResponse<TransactionDto>> CreateTransactionAsync(string userId, int accountId, UpsertTransactionDto dto);
    // TODO: Define methods for Update and Delete
}

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public TransactionService(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
        {
            return ApiResponse<PaginatedResult<TransactionDto>>.Failure("Account not found.");
        }

        var pagedData = await _transactionRepository.GetPagedTransactionsForAccountAsync(accountId, queryParams);
        var pagedDto = new PaginatedResult<TransactionDto>(
            _mapper.Map<List<TransactionDto>>(pagedData.Data),
            pagedData.TotalRecords,
            pagedData.PageNumber,
            pagedData.PageSize
        );

        return ApiResponse<PaginatedResult<TransactionDto>>.Success(pagedDto);
    }

    public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(string userId, int accountId, UpsertTransactionDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
        {
            return ApiResponse<TransactionDto>.Failure("Account not found.");
        }

        var transaction = new Transaction
        {
            Description = dto.Description,
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            AccountId = accountId,
            CategoryId = dto.CategoryId
        };

        if (transaction.Type == Domain.Enums.TransactionType.Income)
        {
            account.Balance += transaction.Amount;
        }
        else
        {
            account.Balance -= transaction.Amount;
        }

        var newTransaction = await _transactionRepository.AddAsync(transaction);
        _accountRepository.Update(account); // Note: This uses two separate SaveChanges calls. A Unit of Work pattern would improve this.

        return ApiResponse<TransactionDto>.Success(_mapper.Map<TransactionDto>(newTransaction));
    }
}