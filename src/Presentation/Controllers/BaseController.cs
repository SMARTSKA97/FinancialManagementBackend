using Application;
using Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize]
[EnableRateLimiting("api")]
public abstract class BaseController : ControllerBase
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value is null ? NotFound() : Ok(ApiResult<T>.Success(result.Value));
        }

        // Return 200 OK even for failures, wrapped in the Result object to mask the error status.
        // We MUST NOT return 'result' directly because the serializer will try to access 'Value', which throws on failure.
        return Ok(ApiResult<T>.Failure(result.Error)); 
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return Ok(result);
    }

}
