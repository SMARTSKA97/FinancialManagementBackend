using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.Accounts;

public class UpdateAccountDto
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("balance")]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; }
}