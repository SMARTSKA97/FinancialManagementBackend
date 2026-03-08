using Application;
using Application.Common.Models;
using Application.DTOs.Budgets;
using Application.Features.Budgets.Commands;
using Application.Features.Budgets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class BudgetsController : BaseController
{
    private readonly ISender _sender;

    public BudgetsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("search")]
    public async Task<IActionResult> GetPaged([FromBody] QueryParameters query)
    {
        var result = await _sender.Send(new GetBudgetsQuery(UserId, query));
        return HandleResult(result);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] DateTime? date)
    {
        var requestDate = date ?? DateTime.UtcNow;
        var result = await _sender.Send(new GetBudgetProgressQuery(UserId, requestDate));
        return HandleResult(result);
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] UpsertBudgetDto dto)
    {
        var result = await _sender.Send(new UpsertBudgetCommand(UserId, dto.Id, dto));
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertBudgetDto dto)
    {
        var result = await _sender.Send(new UpsertBudgetCommand(UserId, id, dto));
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _sender.Send(new DeleteBudgetCommand(UserId, id));
        return HandleResult(result);
    }
}
