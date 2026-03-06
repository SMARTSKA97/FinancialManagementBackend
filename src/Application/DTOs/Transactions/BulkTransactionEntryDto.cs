using System.Text.Json.Serialization;

namespace Application.DTOs.Transactions;

public class BulkTransactionEntryDto
{
    [JsonPropertyName("accountId")]
    public int AccountId { get; set; }

    [JsonPropertyName("destinationAccountId")]
    public int? DestinationAccountId { get; set; }

    [JsonPropertyName("transaction")]
    public required UpsertTransactionDto Transaction { get; set; }
}
