using System.Text.Json.Serialization;

namespace Application.DTOs.Dashboard;

public class GroupExtremeDto
{
    [JsonPropertyName("groupName")]
    public required string GroupName { get; set; }

    [JsonPropertyName("maxTransaction")]
    public BasicTransactionDto? MaxTransaction { get; set; }

    [JsonPropertyName("minTransaction")]
    public BasicTransactionDto? MinTransaction { get; set; }
}
