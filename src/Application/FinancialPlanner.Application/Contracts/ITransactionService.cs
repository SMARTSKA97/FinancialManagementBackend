using FinancialPlanner.Application.DTOs.Transactions;

namespace FinancialPlanner.Application.Contracts;

public interface ITransactionService
{
    Task<ApiResponse<PaginatedResult<TransactionDto>>> GetTransactionsAsync(string userId, int accountId, QueryParameters queryParams);
    Task<ApiResponse<TransactionDto>> UpsertTransactionAsync(string userId, int accountId, UpsertTransactionDto dto);
    Task<ApiResponse<bool>> DeleteTransactionAsync(string userId, int accountId, int transactionId);
    Task<ApiResponse<bool>> CreateTransferAsync(string userId, int sourceAccountId, CreateTransferDto dto);
}