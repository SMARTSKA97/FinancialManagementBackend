using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Budgets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Budgets.Queries;

public record GetBudgetProgressQuery(string UserId, DateTime RequestDate) : IRequest<Result<List<BudgetProgressDto>>>;

public class GetBudgetProgressQueryHandler : IRequestHandler<GetBudgetProgressQuery, Result<List<BudgetProgressDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBudgetProgressQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BudgetProgressDto>>> Handle(GetBudgetProgressQuery request, CancellationToken cancellationToken)
    {
        // Calculate Month Start/End for better filtering
        var monthStart = new DateTime(request.RequestDate.Year, request.RequestDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        // Get all budgets that are active at any point in this month
        var activeBudgets = await _context.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == request.UserId && 
                       b.StartDate <= monthEnd && 
                       (b.EndDate == null || b.EndDate >= monthStart))
            .Include(b => b.TransactionCategory)
            .ToListAsync(cancellationToken);

        if (!activeBudgets.Any())
        {
            return Result<List<BudgetProgressDto>>.Success(new List<BudgetProgressDto>());
        }

        var progressList = new List<BudgetProgressDto>();

        foreach (var budget in activeBudgets)
        {
            DateTime periodStart;
            DateTime periodEnd;

            // Determine the time window based on the period (Monthly vs Yearly)
            if (budget.Period == Domain.Enums.BudgetPeriod.Monthly)
            {
                periodStart = new DateTime(request.RequestDate.Year, request.RequestDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                periodEnd = periodStart.AddMonths(1).AddTicks(-1);
            }
            else // Yearly
            {
                periodStart = new DateTime(request.RequestDate.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                periodEnd = periodStart.AddYears(1).AddTicks(-1);
            }

            // Get user's account IDs to filter transactions correctly
            var accountIds = await _context.Accounts
                .Where(a => a.UserId == request.UserId)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);

            // Calculate spent amount
            // A budget can be overall (TransactionCategoryId is null) or specific to a category
            var spentQuery = _context.Transactions
                .AsNoTracking()
                .Where(t => accountIds.Contains(t.AccountId) && t.Date >= periodStart && t.Date <= periodEnd);

            if (budget.TransactionCategoryId.HasValue)
            {
                spentQuery = spentQuery.Where(t => t.TransactionCategoryId == budget.TransactionCategoryId.Value);
            }

            // We sum the transactions where Type is Expense
            var expenses = await spentQuery
                .Where(t => t.Type == Domain.Enums.TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;
                
            var spentAmount = Math.Abs(expenses);

            progressList.Add(new BudgetProgressDto
            {
                BudgetId = budget.Id,
                TransactionCategoryId = budget.TransactionCategoryId,
                CategoryName = budget.TransactionCategory?.Name,
                BudgetAmount = budget.Amount,
                SpentAmount = spentAmount,
                Period = budget.Period,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate
            });
        }

        progressList = progressList
            .OrderByDescending(p => p.PercentageUsed)
            .ToList();

        return Result<List<BudgetProgressDto>>.Success(progressList);
    }
}
