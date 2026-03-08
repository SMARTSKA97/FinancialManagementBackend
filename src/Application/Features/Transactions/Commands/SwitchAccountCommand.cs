using Application.Common.Models;
using Application.Contracts;
using MediatR;

namespace Application.Features.Transactions.Commands;

public record SwitchAccountCommand(string UserId, int TransactionId, int DestinationAccountId) 
    : IRequest<Result<bool>>;

public class SwitchAccountCommandHandler : IRequestHandler<SwitchAccountCommand, Result<bool>>
{
    private readonly ITransactionService _transactionService;

    public SwitchAccountCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<bool>> Handle(SwitchAccountCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.SwitchAccountAsync(request.UserId, request.TransactionId, request.DestinationAccountId);
    }
}
