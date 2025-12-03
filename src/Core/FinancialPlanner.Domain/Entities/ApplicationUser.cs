using Microsoft.AspNetCore.Identity;

namespace FinancialPlanner.Domain.Entities;

public class ApplicationUser : IdentityUser, IAuditable
{
    public required string Name { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastSeenUtc { get; set; }
    
    // Session Tracking
    public string? CurrentSessionId { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public string? LastKnownIp { get; set; }
    public string? LastKnownUserAgent { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
}