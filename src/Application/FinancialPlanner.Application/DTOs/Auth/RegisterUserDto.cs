using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinancialPlanner.Application.DTOs.Auth;

public class RegisterUserDto
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }
}