namespace FinancialPlanner.Application.DTOs.TransactionCategory;

public class UpsertTransactionCategoryDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
}