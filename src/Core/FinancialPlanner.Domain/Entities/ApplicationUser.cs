using Microsoft.AspNetCore.Identity;

namespace FinancialPlanner.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public required string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}