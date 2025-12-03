namespace FinancialPlanner.Application.Contracts;

public class RefreshTokenData
{
    public required string UserId { get; set; }
    public required string Token { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public required string CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}
