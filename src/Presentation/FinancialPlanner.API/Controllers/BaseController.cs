using FinancialPlanner.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected IActionResult HandleApiResponse<T>(ApiResponse<T> response)
    {
        if (response.IsSuccess)
        {
            return Ok(response);
        }

        // You can expand this to check for specific error types if needed
        return BadRequest(response);
    }
}
