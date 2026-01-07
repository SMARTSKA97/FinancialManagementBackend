namespace FinancialPlanner.Application.Services;

using AutoMapper;
using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Transactions;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class TransactionService : ITransactionService
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public TransactionService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.UserId != userId)
            return Result.Failure<PaginatedResult<TransactionDto>>(new Error("Account.NotFound", "Account not found."));

        var queryable = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .Include(t => t.TransactionCategory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParams.GlobalSearch))
        {
            var search = queryParams.GlobalSearch.Trim();
            queryable = queryable.Where(t => t.Description.Contains(search) ||
                (t.TransactionCategory != null && t.TransactionCategory.Name.Contains(search)));
        }

        var totalRecords = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(t => t.Date)
            .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var pagedDto = new PaginatedResult<TransactionDto>(
            _mapper.Map<List<TransactionDto>>(items),
            totalRecords,
            queryParams.PageNumber,
            queryParams.PageSize
        );
        return Result.Success(pagedDto);
    }

    public async Task<Result<TransactionDto>> UpsertTransactionAsync(string userId, int accountId, UpsertTransactionDto dto)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.UserId != userId)
            return Result.Failure<TransactionDto>(new Error("Account.NotFound", "Account not found."));

        Transaction? transaction = null;
        decimal oldAmount = 0;
        TransactionType oldType = TransactionType.Expense;

        if (dto.Id.HasValue && dto.Id.Value > 0)
        {
            transaction = await _context.Transactions.FindAsync(dto.Id.Value);
            if (transaction == null || transaction.AccountId != accountId)
                return Result.Failure<TransactionDto>(new Error("Transaction.NotFound", "Transaction not found."));

            oldAmount = transaction.Amount;
            oldType = transaction.Type;
            _mapper.Map(dto, transaction);
            _context.Transactions.Update(transaction);
        }
        else
        {
            transaction = _mapper.Map<Transaction>(dto);
            transaction.AccountId = accountId;
            _context.Transactions.Add(transaction);
        }

        // Adjust Account Balance
        account.Balance += (oldType == TransactionType.Income ? -oldAmount : oldAmount);
        account.Balance += (transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount);

        _context.Accounts.Update(account);
        
        await _context.SaveChangesAsync();

        // Reload to include relations for return DTO
        var resultTransactionWithCategory = await _context.Transactions
            .Include(t => t.TransactionCategory)
            .FirstAsync(t => t.Id == transaction.Id);

        var resultDto = _mapper.Map<TransactionDto>(resultTransactionWithCategory);

        return Result.Success(resultDto);
    }

    public async Task<Result<bool>> DeleteTransactionAsync(string userId, int accountId, int transactionId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null || account.UserId != userId)
            return Result.Failure<bool>(new Error("Account.NotFound", "Account not found."));

        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null || transaction.AccountId != accountId)
            return Result.Failure<bool>(new Error("Transaction.NotFound", "Transaction not found."));

        // Revert Balance
        account.Balance += (transaction.Type == TransactionType.Income ? -transaction.Amount : transaction.Amount);

        _context.Accounts.Update(account);
        _context.Transactions.Remove(transaction);

        await _context.SaveChangesAsync();

        return Result.Success(true);
    }

    public async Task<Result<bool>> CreateTransferAsync(string userId, int sourceAccountId, CreateTransferDto dto)
    {
        var sourceAccount = await _context.Accounts.FindAsync(sourceAccountId);
        if (sourceAccount == null || sourceAccount.UserId != userId)
            return Result.Failure<bool>(new Error("Account.NotFound", "Source account not found."));

        var destinationAccount = await _context.Accounts.FindAsync(dto.DestinationAccountId);
        if (destinationAccount == null || destinationAccount.UserId != userId)
            return Result.Failure<bool>(new Error("Account.NotFound", "Destination account not found."));

        if (sourceAccountId == dto.DestinationAccountId)
            return Result.Failure<bool>(new Error("Transfer.SameAccount", "Cannot transfer to the same account."));

        var expenseTransaction = new Transaction
        {
            Description = $"Transfer to {destinationAccount.Name}",
            Amount = dto.Amount,
            Date = dto.Date,
            Type = TransactionType.Expense,
            AccountId = sourceAccountId,
            TransactionCategoryId = dto.TransactionCategoryId
        };
        sourceAccount.Balance -= dto.Amount;

        var incomeTransaction = new Transaction
        {
            Description = $"Transfer from {sourceAccount.Name}",
            Amount = dto.Amount,
            Date = dto.Date,
            Type = TransactionType.Income,
            AccountId = dto.DestinationAccountId,
            TransactionCategoryId = dto.TransactionCategoryId
        };
        destinationAccount.Balance += dto.Amount;

        _context.Transactions.Add(expenseTransaction);
        _context.Transactions.Add(incomeTransaction);
        _context.Accounts.Update(sourceAccount);
        _context.Accounts.Update(destinationAccount);

        await _context.SaveChangesAsync(); // Transactional by default

        return Result.Success(true);
    }

    public async Task<Result<bool>> SwitchAccountAsync(string userId, int transactionId, int destinationAccountId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null)
            return Result.Failure<bool>(new Error("Transaction.NotFound", "Transaction not found."));

        var sourceAccount = await _context.Accounts.FindAsync(transaction.AccountId);
        var destinationAccount = await _context.Accounts.FindAsync(destinationAccountId);

        if (sourceAccount == null || destinationAccount == null || sourceAccount.UserId != userId || destinationAccount.UserId != userId)
            return Result.Failure<bool>(new Error("Account.NotFound", "One or both accounts could not be found."));

        if (sourceAccount.Id == destinationAccount.Id)
            return Result.Failure<bool>(new Error("Switch.SameAccount", "Cannot switch to the same account."));

        // Revert from source
        sourceAccount.Balance += (transaction.Type == TransactionType.Income ? -transaction.Amount : transaction.Amount);

        // Apply to destination
        destinationAccount.Balance += (transaction.Type == TransactionType.Income ? transaction.Amount : -transaction.Amount);

        // Move transaction
        transaction.AccountId = destinationAccountId;

        _context.Accounts.Update(sourceAccount);
        _context.Accounts.Update(destinationAccount);
        _context.Transactions.Update(transaction);

        await _context.SaveChangesAsync();

        return Result.Success(true);
    }
}