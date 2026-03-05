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

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(string userId)
    {
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

        return Result.Success(summary);
    }

    public async Task<Result<List<SpendingByCategoryDto>>> GetSpendingByCategoryAsync(string userId)
    {
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

        return Result.Success(spendingData);
    }

    public async Task<Result<AccountSummaryDto>> GetAccountSummaryAsync(string userId, int accountId)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

        if (account == null)
            return Result.Failure<AccountSummaryDto>(new Error("Account.NotFound", "Account not found."));

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

        return Result.Success(summary);
    }
}
