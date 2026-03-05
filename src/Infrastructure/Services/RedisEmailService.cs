using Application.Contracts;
using Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services;

public class RedisEmailService : IEmailService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEmailService> _logger;
    private const string QueueName = "MailQueue";

    public RedisEmailService(IConnectionMultiplexer redis, ILogger<RedisEmailService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task SendEmailAsync(MailRequest mailRequest)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(mailRequest);
        await db.ListRightPushAsync(QueueName, json);
        _logger.LogInformation("Email to {To} queued into Redis.", mailRequest.To);
    }
}
