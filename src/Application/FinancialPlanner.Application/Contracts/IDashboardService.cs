using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Dashboard;

namespace FinancialPlanner.Application.Contracts;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(string userId);
    Task<Result<List<SpendingByCategoryDto>>> GetSpendingByCategoryAsync(string userId);
    Task<Result<AccountSummaryDto>> GetAccountSummaryAsync(string userId, int accountId);
}
