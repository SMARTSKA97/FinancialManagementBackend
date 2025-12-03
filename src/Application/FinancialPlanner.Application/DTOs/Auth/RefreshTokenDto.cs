using System.ComponentModel.DataAnnotations;

namespace FinancialPlanner.Application.DTOs.Auth;

public class RefreshTokenDto
{
    public required string RefreshToken { get; set; }
}