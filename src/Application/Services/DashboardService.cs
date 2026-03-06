using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Dashboard;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(IApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<PublicStatsDto>> GetPublicStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalAccounts = await _context.Accounts.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();

        return Result.Success(new PublicStatsDto
        {
            TotalUsers = totalUsers,
            TotalAccounts = totalAccounts,
            TotalTransactions = totalTransactions
        });
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        var startFilter = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endFilter = endDate ?? startFilter.AddMonths(1).AddDays(-1);

        var netWorth = await _context.Accounts
            .Where(a => a.UserId == userId)
            .SumAsync(a => a.Balance);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.Account.UserId == userId && t.Date >= startFilter && t.Date <= endFilter);

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

        return Result.Success(summary);
    }

    public async Task<Result<List<SpendingByCategoryDto>>> GetSpendingByCategoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        var startFilter = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endFilter = endDate ?? startFilter.AddMonths(1).AddDays(-1);

        var spendingData = await _context.Transactions
            .Where(t =>
                t.Account.UserId == userId &&
                t.Type == TransactionType.Expense &&
                t.Date >= startFilter && t.Date <= endFilter)
            .GroupBy(t => t.TransactionCategory != null ? t.TransactionCategory.Name : "Uncategorized")
            .Select(group => new SpendingByCategoryDto
            {
                CategoryName = group.Key,
                TotalAmount = group.Sum(t => t.Amount)
            })
            .OrderByDescending(d => d.TotalAmount)
            .ToListAsync();

        return Result.Success(spendingData);
    }

    public async Task<Result<AccountSummaryDto>> GetAccountSummaryAsync(string userId, int accountId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

        if (account == null)
            return Result.Failure<AccountSummaryDto>(new Error("Account.NotFound", "Account not found."));

        var now = DateTime.UtcNow;
        var startFilter = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endFilter = endDate ?? startFilter.AddMonths(1).AddDays(-1);

        var monthlyTransactions = _context.Transactions
            .Where(t => t.AccountId == accountId && t.Date >= startFilter && t.Date <= endFilter);

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

        return Result.Success(summary);
    }

    public async Task<Result<DashboardInsightsDto>> GetDeepInsightsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;
        var startFilter = startDate ?? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endFilter = endDate ?? startFilter.AddMonths(1).AddDays(-1);

        var query = _context.Transactions
            .Include(t => t.TransactionCategory)
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Date >= startFilter && t.Date <= endFilter);

        var insights = new DashboardInsightsDto();

        // High volume querying logic mapping to BasicTransactionDto
        var baseSet = await query.Select(t => new BasicTransactionDto
        {
            Id = t.Id,
            Description = t.Description,
            Amount = t.Amount,
            Date = t.Date,
            Type = (int)t.Type,
            CategoryName = t.TransactionCategory != null ? t.TransactionCategory.Name : "Uncategorized",
            AccountName = t.Account.Name
        }).ToListAsync();

        if (!baseSet.Any()) 
            return Result.Success(insights);

        // Highest and Lowest
        insights.HighestAmountTransactions = baseSet.OrderByDescending(t => t.Amount).Take(5).ToList();
        insights.LowestAmountTransactions = baseSet.OrderBy(t => t.Amount).Take(5).ToList();

        // Latest and Oldest
        insights.LatestTransactions = baseSet.OrderByDescending(t => t.Date).Take(5).ToList();
        insights.OldestTransactions = baseSet.OrderBy(t => t.Date).Take(5).ToList();

        // Category Extremes
        var categoryGroups = baseSet.GroupBy(t => t.CategoryName);
        foreach (var group in categoryGroups.Where(g => g.Key != null))
        {
            insights.CategoryExtremes.Add(new GroupExtremeDto
            {
                GroupName = group.Key!,
                MaxTransaction = group.OrderByDescending(t => t.Amount).FirstOrDefault(),
                MinTransaction = group.OrderBy(t => t.Amount).FirstOrDefault()
            });
        }

        // Account Extremes
        var accountGroups = baseSet.GroupBy(t => t.AccountName);
        foreach (var group in accountGroups.Where(g => g.Key != null))
        {
            insights.AccountExtremes.Add(new GroupExtremeDto
            {
                GroupName = group.Key!,
                MaxTransaction = group.OrderByDescending(t => t.Amount).FirstOrDefault(),
                MinTransaction = group.OrderBy(t => t.Amount).FirstOrDefault()
            });
        }

        return Result.Success(insights);
    }
}
