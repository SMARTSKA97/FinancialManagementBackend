namespace Application.DTOs.Auth;

public class AuthResponseDto
{
    public required string Token { get; set; }
    // We will add the refresh token here later
}