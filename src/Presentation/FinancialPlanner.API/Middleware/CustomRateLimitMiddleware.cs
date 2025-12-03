using FinancialPlanner.Application.Contracts;

namespace FinancialPlanner.API.Middleware
{
    public class CustomRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRedisRateLimiter? _rateLimiter;
        private readonly ILogger<CustomRateLimitMiddleware> _logger;
        private readonly int _maxRequests = 5; // 5 requests per window
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1); // 1 minute window

        public CustomRateLimitMiddleware(
            RequestDelegate next, 
            ILogger<CustomRateLimitMiddleware> logger,
            IRedisRateLimiter? rateLimiter = null)
        {
            _next = next;
            _logger = logger;
            _rateLimiter = rateLimiter;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Apply only to the login endpoint
            if (context.Request.Path.StartsWithSegments("/api/Auth/login", StringComparison.OrdinalIgnoreCase) 
                && HttpMethods.IsPost(context.Request.Method))
            {
                var clientId = GetClientIdentifier(context);
                var endpoint = "/api/Auth/login";

                // If Redis rate limiter is available, use it (distributed)
                if (_rateLimiter != null)
                {
                    // ⭐ OPTIMIZATION: Single atomic call to get everything!
                    var result = await _rateLimiter.CheckLimitAsync(clientId, endpoint, _maxRequests, _window);

                    if (!result.IsAllowed)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        context.Response.ContentType = "application/json";
                        context.Response.Headers["Retry-After"] = ((int)result.ResetTime.TotalSeconds).ToString();

                        var response = new
                        {
                            Message = "Too many login attempts. Please try again later.",
                            RetryAfter = Math.Ceiling(result.ResetTime.TotalSeconds),
                            Limit = _maxRequests,
                            Window = $"{_window.TotalMinutes} minute(s)"
                        };

                        _logger.LogWarning("[RateLimit] ⛔ Blocked login attempt from {ClientId}", clientId);

                        await context.Response.WriteAsJsonAsync(response);
                        return;
                    }

                    // Set response headers from the result
                    context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
                    context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
                    context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(result.ResetTime).ToUnixTimeSeconds().ToString();
                }
                else
                {
                    // Fallback: If Redis is unavailable, allow all requests (fail-open)
                    _logger.LogWarning("[RateLimit] ⚠️ Redis rate limiter unavailable. Allowing request (fail-open).");
                }
            }

            await _next(context);
        }

        private static string GetClientIdentifier(HttpContext context)
        {
            // Use IP address as client identifier
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            // For production behind proxy/load balancer, check forwarded headers
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                ip = forwardedFor.ToString().Split(',')[0].Trim();
            }

            return ip;
        }
    }
}
