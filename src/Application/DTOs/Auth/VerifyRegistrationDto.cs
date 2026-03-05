using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class VerifyRegistrationDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Otp { get; set; }
}
