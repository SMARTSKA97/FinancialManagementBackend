using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Domain.Entities;

[Owned] // This tells EF Core it's an "owned" entity type, part of ApplicationUser
public class RefreshToken
{
    public int Id { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public required string CreatedByIp { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public string? RevokedByIp { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresUtc;
    public bool IsActive => RevokedUtc == null && !IsExpired;
}