using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FinancialPlanner.Domain.Enums;

namespace FinancialPlanner.Application.DTOs.Transactions;

public class CreateTransactionDto
{
    [JsonPropertyName("description")]
    [Required]
    public required string Description { get; set; }

    [JsonPropertyName("amount")]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [JsonPropertyName("date")]
    [Required]
    public DateTime Date { get; set; }

    [JsonPropertyName("type")]
    [Required]
    public TransactionType Type { get; set; }
}