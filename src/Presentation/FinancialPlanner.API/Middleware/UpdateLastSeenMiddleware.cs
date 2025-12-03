using FinancialPlanner.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FinancialPlanner.API.Middleware;

public class UpdateLastSeenMiddleware
{
    private readonly RequestDelegate _next;

    public UpdateLastSeenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        // Let the request proceed first
        await _next(context);

        // After the request, check if user was authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                // Optimization: In a high-scale app, you'd cache this or update only every 5 mins.
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Optimization: Update only if more than 5 minutes have passed
                    if (user.LastSeenUtc.AddMinutes(5) < DateTime.UtcNow)
                    {
                        user.LastSeenUtc = DateTime.UtcNow;
                        try
                        {
                            await userManager.UpdateAsync(user);
                        }
                        catch
                        {
                            // Ignore errors during background update to prevent request failure
                        }
                    }
                }
            }
        }
    }
}