using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Transactions;
using MediatR;

namespace Application.Features.Transactions.Commands;

public record CreateTransferCommand(string UserId, int AccountId, CreateTransferDto Transfer) 
    : IRequest<Result<bool>>;

public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, Result<bool>>
{
    private readonly ITransactionService _transactionService;

    public CreateTransferCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<bool>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.CreateTransferAsync(request.UserId, request.AccountId, request.Transfer);
    }
}
