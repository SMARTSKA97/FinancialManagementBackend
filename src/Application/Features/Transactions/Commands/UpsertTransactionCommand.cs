using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Transactions;
using MediatR;

namespace Application.Features.Transactions.Commands;

public record UpsertTransactionCommand(string UserId, int AccountId, UpsertTransactionDto Transaction) 
    : IRequest<Result<TransactionDto>>;

public class UpsertTransactionCommandHandler : IRequestHandler<UpsertTransactionCommand, Result<TransactionDto>>
{
    private readonly ITransactionService _transactionService;

    public UpsertTransactionCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<TransactionDto>> Handle(UpsertTransactionCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.UpsertTransactionAsync(request.UserId, request.AccountId, request.Transaction);
    }
}
