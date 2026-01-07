using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Transactions;

namespace FinancialPlanner.Application.Contracts;

public interface ITransactionService
{
    Task<Result<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams);
    Task<Result<TransactionDto>> UpsertTransactionAsync(string userId, int accountId, UpsertTransactionDto dto);
    Task<Result<bool>> DeleteTransactionAsync(string userId, int accountId, int transactionId);
    Task<Result<bool>> CreateTransferAsync(string userId, int sourceAccountId, CreateTransferDto dto);
    Task<Result<bool>> SwitchAccountAsync(string userId, int transactionId, int destinationAccountId);
}