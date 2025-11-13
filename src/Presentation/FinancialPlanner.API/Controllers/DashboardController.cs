using FinancialPlanner.Application;
using FinancialPlanner.Application.DTOs.Dashboard;
using FinancialPlanner.Domain.Enums;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var netWorth = await _context.Accounts
            .Where(a => a.UserId == userId)
            .SumAsync(a => a.Balance);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.Account.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth);

        var monthlyIncome = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var monthlyExpenses = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        var summary = new DashboardSummaryDto
        {
            NetWorth = netWorth,
            MonthlyIncome = monthlyIncome,
            MonthlyExpenses = monthlyExpenses
        };

        // --- THIS IS THE FIX ---
        // Wrap the response in the standard ApiResponse envelope
        return Ok(ApiResponse<DashboardSummaryDto>.Success(summary));
    }

    [HttpGet("spending-by-category")]
    public async Task<IActionResult> GetSpendingByCategory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var spendingData = await _context.Transactions
            .Where(t =>
                t.Account.UserId == userId &&
                t.Type == TransactionType.Expense &&
                t.Date >= startOfMonth && t.Date <= endOfMonth)
            .GroupBy(t => t.TransactionCategory != null ? t.TransactionCategory.Name : "Uncategorized")
            .Select(group => new SpendingByCategoryDto
            {
                CategoryName = group.Key,
                TotalAmount = group.Sum(t => t.Amount)
            })
            .OrderByDescending(d => d.TotalAmount)
            .ToListAsync();

        // --- THIS IS THE FIX ---
        // Wrap the response in the standard ApiResponse envelope
        return Ok(ApiResponse<List<SpendingByCategoryDto>>.Success(spendingData));
    }

    [HttpGet("account-summary/{accountId}")]
    public async Task<IActionResult> GetAccountSummary(int accountId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

        if (account == null)
            return NotFound(ApiResponse<object>.Failure("Account not found."));

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.AccountId == accountId && t.Date >= startOfMonth && t.Date <= endOfMonth);

        var totalIncome = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var totalExpenses = await monthlyTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        var summary = new AccountSummaryDto
        {
            CurrentBalance = account.Balance,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses
        };

        // This endpoint was already correct, but we leave it for consistency
        return Ok(ApiResponse<AccountSummaryDto>.Success(summary));
    }
}