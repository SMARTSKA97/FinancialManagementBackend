using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RefreshTokenDto
{
    public required string RefreshToken { get; set; }
}