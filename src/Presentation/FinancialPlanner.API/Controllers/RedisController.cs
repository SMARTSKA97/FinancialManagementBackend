using Microsoft.AspNetCore.Mvc;
using FinancialPlanner.Application.Contracts;

namespace FinancialPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly ILogger<RedisController> _logger;

    public RedisController(IRedisService redisService, ILogger<RedisController> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint - Test Redis connectivity
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var isHealthy = await _redisService.PingAsync();
            
            if (isHealthy)
            {
                return Ok(new
                {
                    status = "healthy",
                    message = "Redis connection is working",
                    timestamp = DateTime.UtcNow
                });
            }
            
            return StatusCode(503, new
            {
                status = "unhealthy",
                message = "Redis ping failed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Test endpoint - Set a test key in Redis
    /// </summary>
    [HttpPost("test/set")]
    public async Task<IActionResult> SetTestKey([FromBody] TestKeyRequest request)
    {
        try
        {
            var expiry = TimeSpan.FromMinutes(request.ExpiryMinutes ?? 5);
            var result = await _redisService.SetStringAsync(
                $"test:{request.Key}",
                request.Value,
                expiry
            );

            if (result)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Key 'test:{request.Key}' set successfully",
                    expiry = expiry.TotalMinutes
                });
            }

            return BadRequest(new { success = false, message = "Failed to set key" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set test key");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint - Get a test key from Redis
    /// </summary>
    [HttpGet("test/get/{key}")]
    public async Task<IActionResult> GetTestKey(string key)
    {
        try
        {
            var value = await _redisService.GetStringAsync($"test:{key}");

            if (value != null)
            {
                return Ok(new
                {
                    success = true,
                    key = $"test:{key}",
                    value = value
                });
            }

            return NotFound(new
            {
                success = false,
                message = $"Key 'test:{key}' not found or expired"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get test key");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint - Delete a test key from Redis
    /// </summary>
    [HttpDelete("test/delete/{key}")]
    public async Task<IActionResult> DeleteTestKey(string key)
    {
        try
        {
            var result = await _redisService.DeleteKeyAsync($"test:{key}");

            return Ok(new
            {
                success = result,
                message = result ? $"Key 'test:{key}' deleted successfully" : "Key not found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete test key");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

public class TestKeyRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int? ExpiryMinutes { get; set; }
}
