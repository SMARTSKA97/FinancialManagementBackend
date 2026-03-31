using Application.Common.Interfaces;
using Application.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cachedResponse = await _cacheService.GetAsync<TResponse>(request.CacheKey);

        if (cachedResponse != null)
        {
            _logger.LogInformation("Fetched from Cache -> '{CacheKey}'.", request.CacheKey);
            return cachedResponse;
        }

        var response = await next();

        await _cacheService.SetAsync(request.CacheKey, response, request.Expiration ?? TimeSpan.FromHours(1));

        return response;
    }
}
