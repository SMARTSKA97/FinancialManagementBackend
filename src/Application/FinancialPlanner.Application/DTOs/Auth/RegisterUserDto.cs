using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinancialPlanner.Application.DTOs.Auth;

public class RegisterUserDto
{
    [JsonPropertyName("userName")] 
    [Required]
    public required string UserName { get; set; }

    [JsonPropertyName("email")] 
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [JsonPropertyName("password")]
    [Required]
    public required string Password { get; set; }
}