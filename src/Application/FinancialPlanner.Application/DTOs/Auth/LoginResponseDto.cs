using System.ComponentModel.DataAnnotations;

namespace FinancialPlanner.Application.DTOs.Auth;

public class LoginResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string UserName { get; set; }
}