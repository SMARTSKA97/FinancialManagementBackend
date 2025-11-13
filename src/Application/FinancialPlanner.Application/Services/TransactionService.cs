using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;


namespace FinancialPlanner.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams)
    {
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<PaginatedResult<TransactionDto>>.Failure("Account not found.");

        // Call the repository via the Unit of Work
        var pagedData = await _unitOfWork.TransactionRepository.GetPagedTransactionsForAccountAsync(accountId, queryParams);
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
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<TransactionDto>.Failure("Account not found.");

        Transaction transaction;
        decimal oldAmount = 0;
        TransactionType oldType = TransactionType.Expense;

        if (dto.Id.HasValue && dto.Id.Value > 0)
        {
            transaction = (await _unitOfWork.TransactionRepository.GetByIdAsync(dto.Id.Value))!;
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

        _unitOfWork.AccountRepository.Upsert(account); // Stage update
        _unitOfWork.TransactionRepository.Upsert(transaction); // Stage update/create

        await _unitOfWork.CompleteAsync(); // Save all changes

        var resultTransactionWithCategory = await _unitOfWork.TransactionRepository.GetByIdAsync(transaction.Id, includeRelated: true);
        var resultDto = _mapper.Map<TransactionDto>(resultTransactionWithCategory);

        return ApiResponse<TransactionDto>.Success(resultDto);
    }

    public async Task<ApiResponse<bool>> DeleteTransactionAsync(string userId, int accountId, int transactionId)
    {
        var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
            return ApiResponse<bool>.Failure("Account not found.");

        var transaction = await _unitOfWork.TransactionRepository.GetByIdAsync(transactionId);
        if (transaction == null || transaction.AccountId != accountId)
            return ApiResponse<bool>.Failure("Transaction not found.");

        account.Balance += (transaction.Type == TransactionType.Income ? -transaction.Amount : transaction.Amount);

        _unitOfWork.AccountRepository.Upsert(account); // Stage update
        _unitOfWork.TransactionRepository.Delete(transaction); // Stage delete

        await _unitOfWork.CompleteAsync(); // Save all changes

        return ApiResponse<bool>.Success(true, "Transaction deleted successfully.");
    }

    public async Task<ApiResponse<bool>> CreateTransferAsync(string userId, int sourceAccountId, CreateTransferDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var sourceAccount = await _unitOfWork.AccountRepository.GetByIdAsync(sourceAccountId);
            if (sourceAccount == null || sourceAccount.UserId != userId)
                return ApiResponse<bool>.Failure("Source account not found.");

            var destinationAccount = await _unitOfWork.AccountRepository.GetByIdAsync(dto.DestinationAccountId);
            if (destinationAccount == null || destinationAccount.UserId != userId)
                return ApiResponse<bool>.Failure("Destination account not found.");

            if (sourceAccountId == dto.DestinationAccountId)
                return ApiResponse<bool>.Failure("Cannot transfer to the same account.");

            // --- THIS IS THE FIX (CS9035) ---
            // We must set all 'required' properties
            var expenseTransaction = new Transaction
            {
                Description = $"Transfer to {destinationAccount.Name}", // Set required property
                Amount = dto.Amount,
                Date = dto.Date,
                Type = TransactionType.Expense,
                AccountId = sourceAccountId,
                TransactionCategoryId = dto.TransactionCategoryId
            };
            sourceAccount.Balance -= dto.Amount;

            var incomeTransaction = new Transaction
            {
                Description = $"Transfer from {sourceAccount.Name}", // Set required property
                Amount = dto.Amount,
                Date = dto.Date,
                Type = TransactionType.Income,
                AccountId = dto.DestinationAccountId,
                TransactionCategoryId = dto.TransactionCategoryId
            };
            destinationAccount.Balance += dto.Amount;

            // Stage all 4 changes
            _unitOfWork.TransactionRepository.Upsert(expenseTransaction);
            _unitOfWork.TransactionRepository.Upsert(incomeTransaction);
            _unitOfWork.AccountRepository.Upsert(sourceAccount);
            _unitOfWork.AccountRepository.Upsert(destinationAccount);

            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitAsync();

            return ApiResponse<bool>.Success(true, "Transfer completed successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponse<bool>.Failure($"Transfer failed: {ex.Message}");
        }
    }
}