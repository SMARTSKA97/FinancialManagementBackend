using Application.Common.Models;
using Application.DTOs.Dashboard;

namespace Application.Contracts;

public interface IDashboardService
{
    Task<Result<PublicStatsDto>> GetPublicStatsAsync();
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(string userId);
    Task<Result<List<SpendingByCategoryDto>>> GetSpendingByCategoryAsync(string userId);
    Task<Result<AccountSummaryDto>> GetAccountSummaryAsync(string userId, int accountId);
}
