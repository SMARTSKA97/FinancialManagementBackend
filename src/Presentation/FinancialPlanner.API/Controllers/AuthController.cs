using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private string GetIpAddress()
    {
        // Check for X-Forwarded-For header if behind a proxy (like Render)
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    [HttpPost("register")]
    [AllowAnonymous] // Explicitly mark this endpoint as public
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous] // Explicitly mark this endpoint as public
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        var result = await _authService.LoginAsync(dto, GetIpAddress(), userAgent);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous] // This must be public because the access token might be expired
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, GetIpAddress());
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous] // Allow logout even if token is expired
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.LogoutAsync(dto.RefreshToken, GetIpAddress());
        return HandleResult(result);
    }
}