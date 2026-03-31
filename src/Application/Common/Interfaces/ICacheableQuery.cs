using MediatR;
using System;

namespace Application.Common.Interfaces;

public interface ICacheableQuery<TResponse> : IRequest<TResponse>
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
}
