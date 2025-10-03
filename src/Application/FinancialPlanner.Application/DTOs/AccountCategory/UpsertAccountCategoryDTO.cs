namespace FinancialPlanner.Application.DTOs.AccountCategory;

public class UpsertAccountCategoryDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
}