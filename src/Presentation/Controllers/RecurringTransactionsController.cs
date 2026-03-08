using Application.Common.Models;
using Application.DTOs.RecurringTransactions;
using Application.Features.RecurringTransactions.Commands;
using Application.Features.RecurringTransactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class RecurringTransactionsController : BaseController
{
    private readonly IMediator _mediator;

    public RecurringTransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HttpPost("search")]
    public async Task<IActionResult> GetRecurringTransactions([FromBody] QueryParameters? query)
    {
        var result = await _mediator.Send(new GetRecurringTransactionsQuery(UserId, query ?? new QueryParameters()));
        return HandleResult(result);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> UpsertRecurringTransaction([FromBody] UpsertRecurringTransactionDto dto)
    {
        var result = await _mediator.Send(new UpsertRecurringTransactionCommand(UserId, dto.Id, dto));
        return HandleResult(result);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteRecurringTransaction([FromBody] DeleteRequest request)
    {
        var result = await _mediator.Send(new DeleteRecurringTransactionCommand(UserId, request.Id));
        return HandleResult(result);
    }
}

public record DeleteRequest(int Id);
