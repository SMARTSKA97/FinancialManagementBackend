using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Budgets;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Budgets.Commands;

public record UpsertBudgetCommand(string UserId, int? BudgetId, UpsertBudgetDto Dto) : IRequest<Result<BudgetDto>>;

public class UpsertBudgetCommandHandler : IRequestHandler<UpsertBudgetCommand, Result<BudgetDto>>
{
    private readonly IApplicationDbContext _context;

    public UpsertBudgetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BudgetDto>> Handle(UpsertBudgetCommand request, CancellationToken cancellationToken)
    {
        Budget? budget = null;
        if (request.BudgetId.HasValue && request.BudgetId.Value > 0)
        {
            budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.UserId == request.UserId, cancellationToken);

            if (budget == null)
            {
                return Result.Failure<BudgetDto>(new Error("Budget.NotFound", "Budget not found."));
            }
        }

        // Check for duplicate active budget (excluding the current one if it's an update)
        var existingBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == request.UserId &&
                                      b.TransactionCategoryId == request.Dto.TransactionCategoryId &&
                                      b.Period == request.Dto.Period &&
                                      (b.EndDate == null || b.EndDate >= DateTime.UtcNow) &&
                                      (budget == null || b.Id != budget.Id), cancellationToken);

        if (existingBudget != null)
        {
            return Result.Failure<BudgetDto>(new Error("Budget.AlreadyExists", "An active budget already exists for this category and period."));
        }

        if (budget == null)
        {
            budget = new Budget
            {
                UserId = request.UserId
            };
            _context.Budgets.Add(budget);
        }

        budget.TransactionCategoryId = request.Dto.TransactionCategoryId;
        budget.Amount = request.Dto.Amount;
        budget.Period = request.Dto.Period;
        budget.StartDate = request.Dto.StartDate;
        budget.EndDate = request.Dto.EndDate;

        await _context.SaveChangesAsync(cancellationToken);

        string? categoryName = null;
        if (budget.TransactionCategoryId.HasValue)
        {
            categoryName = await _context.TransactionCategories
                .Where(c => c.Id == budget.TransactionCategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var dto = new BudgetDto
        {
            Id = budget.Id,
            TransactionCategoryId = budget.TransactionCategoryId,
            CategoryName = categoryName,
            Amount = budget.Amount,
            Period = budget.Period,
            StartDate = budget.StartDate,
            EndDate = budget.EndDate
        };

        return Result<BudgetDto>.Success(dto);
    }
}
