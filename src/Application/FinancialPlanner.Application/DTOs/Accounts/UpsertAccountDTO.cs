namespace FinancialPlanner.Application.DTOs.Accounts;

public class UpsertAccountDto
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public decimal Balance { get; set; }
    public int AccountCategoryId { get; set; }
}