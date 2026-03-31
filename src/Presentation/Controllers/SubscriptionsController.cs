using Application.Features.Subscriptions.Commands;
using Application.Features.Subscriptions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : BaseController
{
    private readonly ISender _sender;

    public SubscriptionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto dto)
    {
        var result = await _sender.Send(new CreateSubscriptionCommand(UserId, dto));
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var result = await _sender.Send(new GetActiveSubscriptionsQuery(UserId));
        return HandleResult(result);
    }
}
