using System.Security.Claims;
using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinancialPlanner.API.Middleware;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = context.User.FindFirst("SessionId")?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(sessionId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // 1. Validate Session ID (Single Session Enforcement)
                    if (user.CurrentSessionId != sessionId)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { Message = "Session expired or logged in from another device." });
                        return;
                    }

                    // 2. Validate IP Address (Session Hijacking Protection)
                    string currentIp;
                    if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                    {
                        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].ToString();
                        currentIp = forwardedHeader.Split(',')[0].Trim();
                    }
                    else
                    {
                        currentIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    }

                    // Note: In a real-world scenario, IP validation might be too strict for mobile users switching networks.
                    // We can relax this or use subnet matching if needed. For now, strict match.
                    if (user.LastKnownIp != currentIp)
                    {
                        // Optional: Log this event as suspicious
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { Message = "Session invalid (IP change detected)." });
                        return;
                    }

                    // 3. Validate User-Agent (Session Hijacking Protection)
                    var currentUserAgent = context.Request.Headers["User-Agent"].ToString();
                    if (user.LastKnownUserAgent != currentUserAgent)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { Message = "Session invalid (User-Agent change detected)." });
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
