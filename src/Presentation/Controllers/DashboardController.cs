using Application.Contracts;
using Application.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("public-stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicStats()
    {
        var result = await _dashboardService.GetPublicStatsAsync();
        return HandleResult(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _dashboardService.GetSummaryAsync(UserId, startDate, endDate);
        return HandleResult(result);
    }

    [HttpGet("spending-by-category")]
    public async Task<IActionResult> GetSpendingByCategory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _dashboardService.GetSpendingByCategoryAsync(UserId, startDate, endDate);
        return HandleResult(result);
    }

    [HttpPost("account-summary")]
    public async Task<IActionResult> GetAccountSummary([FromBody] AccountSummaryRequestDto dto)
    {
        var result = await _dashboardService.GetAccountSummaryAsync(UserId, dto.AccountId, dto.StartDate, dto.EndDate);
        return HandleResult(result);
    }

    [HttpGet("insights")]
    public async Task<IActionResult> GetInsights([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _dashboardService.GetDeepInsightsAsync(UserId, startDate, endDate);
        return HandleResult(result);
    }
}