using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Transactions;
using MediatR;

namespace Application.Features.Transactions.Commands;

public record BulkUpsertTransactionsCommand(string UserId, BulkTransactionPayloadDto Payload) 
    : IRequest<Result<BulkInsertResponseDto>>;

public class BulkUpsertTransactionsCommandHandler : IRequestHandler<BulkUpsertTransactionsCommand, Result<BulkInsertResponseDto>>
{
    private readonly ITransactionService _transactionService;

    public BulkUpsertTransactionsCommandHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<BulkInsertResponseDto>> Handle(BulkUpsertTransactionsCommand request, CancellationToken cancellationToken)
    {
        return _transactionService.BulkUpsertTransactionsAsync(request.UserId, request.Payload);
    }
}
