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
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dashboardService.GetSummaryAsync(UserId);
        return HandleResult(result);
    }

    [HttpGet("spending-by-category")]
    public async Task<IActionResult> GetSpendingByCategory()
    {
        var result = await _dashboardService.GetSpendingByCategoryAsync(UserId);
        return HandleResult(result);
    }

    [HttpPost("account-summary")]
    public async Task<IActionResult> GetAccountSummary([FromBody] AccountSummaryRequestDto dto)
    {
        var result = await _dashboardService.GetAccountSummaryAsync(UserId, dto.AccountId);
        return HandleResult(result);
    }
}