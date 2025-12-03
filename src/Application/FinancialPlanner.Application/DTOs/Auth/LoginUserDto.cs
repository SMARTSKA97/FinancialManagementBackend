using System.ComponentModel.DataAnnotations;

namespace FinancialPlanner.Application.DTOs.Auth;

public class LoginUserDto
{
    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }
    public bool ForceLogin { get; set; } = false;
}