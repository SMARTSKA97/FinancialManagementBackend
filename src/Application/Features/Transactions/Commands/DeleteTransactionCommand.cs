using Application.Common.Models;
using Application.Contracts;
using MediatR;

namespace Application.Features.Transactions.Commands;

public record DeleteTransactionCommand(string UserId, int AccountId, int TransactionId) 
    : IRequest<Result<bool>>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Result<bool>>
{
    private readonly ITransactionService _transactionService;

    public DeleteTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<bool>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.DeleteTransactionAsync(request.UserId, request.AccountId, request.TransactionId);
    }
}
