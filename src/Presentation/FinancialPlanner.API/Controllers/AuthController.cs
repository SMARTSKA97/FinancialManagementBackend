using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.API.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous] // Explicitly mark this endpoint as public
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var response = await _authService.RegisterAsync(dto);
        return HandleApiResponse(response);
    }

    [HttpPost("login")]
    [AllowAnonymous] // Explicitly mark this endpoint as public
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        return HandleApiResponse(response);
    }
}