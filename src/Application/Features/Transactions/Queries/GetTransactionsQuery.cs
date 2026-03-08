using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Transactions;
using MediatR;

namespace Application.Features.Transactions.Queries;

public record GetTransactionsQuery(string UserId, int AccountId, QueryParameters QueryParams) 
    : IRequest<Result<PaginatedResult<TransactionDto>>>;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, Result<PaginatedResult<TransactionDto>>>
{
    private readonly ITransactionService _transactionService;

    public GetTransactionsQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public Task<Result<PaginatedResult<TransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        return _transactionService.GetTransactionsAsync(request.UserId, request.AccountId, request.QueryParams);
    }
}
