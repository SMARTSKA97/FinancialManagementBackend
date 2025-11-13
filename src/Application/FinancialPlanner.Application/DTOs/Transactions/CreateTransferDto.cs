using System.ComponentModel.DataAnnotations;

namespace FinancialPlanner.Application.DTOs.Transactions;

public class CreateTransferDto
{
    [Required]
    public required string Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public int DestinationAccountId { get; set; }

    public int? TransactionCategoryId { get; set; }
}