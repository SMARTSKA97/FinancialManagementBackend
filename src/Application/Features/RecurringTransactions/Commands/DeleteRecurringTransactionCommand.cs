using Application.Common.Models;
using Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RecurringTransactions.Commands;

public record DeleteRecurringTransactionCommand(string UserId, int Id) : IRequest<Result<bool>>;

public class DeleteRecurringTransactionCommandHandler : IRequestHandler<DeleteRecurringTransactionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeleteRecurringTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteRecurringTransactionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.RecurringTransactions
            .FirstOrDefaultAsync(rt => rt.Id == request.Id && rt.UserId == request.UserId, cancellationToken);

        if (entity == null)
        {
            return Result.Failure<bool>(new Error("RecurringTransaction.NotFound", "Recurring transaction not found."));
        }

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        _context.RecurringTransactions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
