using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinancialPlanner.Application.DTOs.Accounts;

public class CreateAccountDto
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("balance")]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }
}