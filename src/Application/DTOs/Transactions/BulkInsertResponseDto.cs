using System.Text.Json.Serialization;

namespace Application.DTOs.Transactions;

public class BulkInsertResponseDto
{
    [JsonPropertyName("successfulCount")]
    public int SuccessfulCount { get; set; }

    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }

    [JsonPropertyName("failedTransactions")]
    public List<BulkInsertFailureDto> FailedTransactions { get; set; } = new();
}

public class BulkInsertFailureDto
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();
}
