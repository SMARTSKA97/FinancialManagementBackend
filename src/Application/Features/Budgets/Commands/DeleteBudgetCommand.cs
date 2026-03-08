using Application.Common.Models;
using Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Budgets.Commands;

public record DeleteBudgetCommand(string UserId, int BudgetId) : IRequest<Result<bool>>;

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteBudgetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.UserId == request.UserId, cancellationToken);

        if (budget == null)
        {
            return Result.Failure<bool>(new Error("Budget.NotFound", "Budget not found."));
        }

        // Soft delete via BaseEntity
        budget.IsDeleted = true;
        budget.DeletedAt = DateTime.UtcNow;

        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
