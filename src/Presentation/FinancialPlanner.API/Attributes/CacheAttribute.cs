using FinancialPlanner.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace FinancialPlanner.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveSeconds;

    public CacheAttribute(int timeToLiveSeconds = 60)
    {
        _timeToLiveSeconds = timeToLiveSeconds;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Check if Cache Service is available
        var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
        if (cacheService == null)
        {
            await next();
            return;
        }

        // 2. Generate Cache Key based on Request Path + Query String
        var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

        // 3. Try Get from Cache (L1 -> L2)
        // We store the raw JSON string in cache for simplicity in this attribute
        var cachedResponse = await cacheService.GetAsync<string>(cacheKey);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            // HIT! Return cached content
            var contentResult = new ContentResult
            {
                Content = cachedResponse,
                ContentType = "application/json",
                StatusCode = 200
            };
            context.Result = contentResult;
            return;
        }

        // 4. Execute Controller Action
        var executedContext = await next();

        // 5. Cache the Result (if successful OKObjectResult)
        if (executedContext.Result is OkObjectResult okObjectResult && okObjectResult.Value != null)
        {
            var json = JsonSerializer.Serialize(okObjectResult.Value);
            await cacheService.SetAsync(cacheKey, json, TimeSpan.FromSeconds(_timeToLiveSeconds));
        }
    }

    private static string GenerateCacheKeyFromRequest(HttpRequest request)
    {
        var keyBuilder = new System.Text.StringBuilder();
        keyBuilder.Append($"{request.Path}");

        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
        {
            keyBuilder.Append($"|{key}-{value}");
        }

        return keyBuilder.ToString();
    }
}
