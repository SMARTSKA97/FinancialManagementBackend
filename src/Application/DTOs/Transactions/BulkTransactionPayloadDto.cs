using System.Text.Json.Serialization;

namespace Application.DTOs.Transactions;

public class BulkTransactionPayloadDto
{
    [JsonPropertyName("transactions")]
    public required List<BulkTransactionEntryDto> Transactions { get; set; }
}
