using Microsoft.Extensions.Caching.Memory;

namespace FinancialPlanner.API.Middleware
{
    public class CustomRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private static readonly object _lock = new(); // Static lock for thread safety

        public CustomRateLimitMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Apply only to the login endpoint
            if (context.Request.Path.StartsWithSegments("/api/Auth/login", StringComparison.OrdinalIgnoreCase) 
                && HttpMethods.IsPost(context.Request.Method))
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var cacheKey = $"RateLimit_{ip}";
                bool shouldBlock = false;
                TimeSpan retryAfter = TimeSpan.Zero;

                // Use a lock to prevent race conditions where multiple requests check and set simultaneously
                lock (_lock)
                {
                    if (_cache.TryGetValue(cacheKey, out DateTime resetTime))
                    {
                        var remaining = resetTime - DateTime.UtcNow;
                        if (remaining > TimeSpan.Zero)
                        {
                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            context.Response.ContentType = "application/json";
                            // Standard Retry-After header (seconds)
                            context.Response.Headers["Retry-After"] = remaining.TotalSeconds.ToString("F0");
                            
                            var response = new
                            {
                                Message = "Too many requests. Please try again later.",
                                RetryAfter = Math.Ceiling(remaining.TotalSeconds)
                            };

                            // We can't await inside a lock, so we write to a memory stream or just return and write outside?
                            // Actually, writing to response inside lock is bad practice (IO).
                            // Better pattern: determine result inside lock, write outside.
                            shouldBlock = true;
                            retryAfter = remaining;
                        }
                    }

                    if (!shouldBlock)
                    {
                        // Allow request and set rate limit for next 1 minute
                        var expiration = DateTime.UtcNow.AddMinutes(1);
                        _cache.Set(cacheKey, expiration, TimeSpan.FromMinutes(1));
                    }
                }

                if (shouldBlock)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    context.Response.Headers["Retry-After"] = retryAfter.TotalSeconds.ToString("F0");
                    
                    var response = new
                    {
                        Message = "Too many requests. Please try again later.",
                        RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                    };

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }

            await _next(context);
        }
    }
}
