using System.Text.Json.Serialization;

namespace Application.DTOs.Dashboard;

public class DashboardInsightsDto
{
    [JsonPropertyName("highestAmountTransactions")]
    public List<BasicTransactionDto> HighestAmountTransactions { get; set; } = new();

    [JsonPropertyName("lowestAmountTransactions")]
    public List<BasicTransactionDto> LowestAmountTransactions { get; set; } = new();

    [JsonPropertyName("latestTransactions")]
    public List<BasicTransactionDto> LatestTransactions { get; set; } = new();

    [JsonPropertyName("oldestTransactions")]
    public List<BasicTransactionDto> OldestTransactions { get; set; } = new();

    [JsonPropertyName("categoryExtremes")]
    public List<GroupExtremeDto> CategoryExtremes { get; set; } = new();

    [JsonPropertyName("accountExtremes")]
    public List<GroupExtremeDto> AccountExtremes { get; set; } = new();
}
