using Application.Contracts;
using Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Insights;

public interface IFinancialInsightsEngine
{
    Task<List<FinancialInsight>> GenerateInsightsAsync(string userId, DateTime endDate);
}

public class FinancialInsightsEngine : IFinancialInsightsEngine
{
    private readonly IEnumerable<IFinancialRule> _rules;
    private readonly IApplicationDbContext _context;

    public FinancialInsightsEngine(IEnumerable<IFinancialRule> rules, IApplicationDbContext context)
    {
        _rules = rules;
        _context = context;
    }

    public async Task<List<FinancialInsight>> GenerateInsightsAsync(string userId, DateTime endDate)
    {
        var startOfMonth = new DateTime(endDate.Year, endDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = startOfMonth.AddMonths(-1);
        
        var currentExpenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endDate && t.Type == Domain.Enums.TransactionType.Expense)
            .SumAsync(t => t.Amount);

        var currentIncome = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endDate && t.Type == Domain.Enums.TransactionType.Income)
            .SumAsync(t => t.Amount);

        var previousExpenses = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= previousMonthStart && t.Date < startOfMonth && t.Type == Domain.Enums.TransactionType.Expense)
            .SumAsync(t => t.Amount);

        var previousIncome = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= previousMonthStart && t.Date < startOfMonth && t.Type == Domain.Enums.TransactionType.Income)
            .SumAsync(t => t.Amount);

        var activeSubscriptions = await _context.Subscriptions
            .Include(s => s.RecurringTransaction)
            .Where(s => s.UserId == userId && s.RecurringTransaction.IsActive)
            .SumAsync(s => s.RecurringTransaction.Amount);

        var budgetLimit = await _context.Budgets
            .Where(b => b.UserId == userId)
            .SumAsync(b => b.Amount);

        var ruleContext = new RuleContext
        {
            UserId = userId,
            TotalIncome = currentIncome,
            TotalExpense = currentExpenses,
            PreviousIncome = previousIncome,
            PreviousExpense = previousExpenses,
            SubscriptionSpend = activeSubscriptions,
            BudgetLimit = budgetLimit
        };

        var results = new List<FinancialInsight>();

        // Built-in rule: delta change
        if (ruleContext.PreviousExpense > 0)
        {
            var delta = ((ruleContext.TotalExpense - ruleContext.PreviousExpense) / ruleContext.PreviousExpense) * 100;
            if (delta > 20)
            {
                results.Add(new FinancialInsight
                {
                    Title = "Danger zone ⚠️ overspending",
                    Message = $"Your spending increased by {delta:F0}% compared to last month.",
                    Type = InsightType.Danger,
                    Triggered = true
                });
            }
            else if (delta < -10)
            {
                results.Add(new FinancialInsight
                {
                    Title = "You stayed under budget 🎯",
                    Message = $"Your spending dropped by {Math.Abs(delta):F0}% compared to last month! Great discipline.",
                    Type = InsightType.Positive,
                    Triggered = true
                });
            }
        }

        foreach (var rule in _rules)
        {
            var insight = rule.Evaluate(ruleContext);
            if (insight.Triggered)
            {
                results.Add(insight);
            }
        }

        return results;
    }
}
