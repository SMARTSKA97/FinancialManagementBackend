using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class FeedbackController : BaseController
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost("upsert")] // Use the standard upsert endpoint
    [AllowAnonymous]   // Allow anyone to submit feedback
    public async Task<IActionResult> Upsert([FromBody] CreateFeedbackDto dto)
    {
        var result = await _feedbackService.CreateFeedbackAsync(dto);
        return HandleResult(result);
    }
}

