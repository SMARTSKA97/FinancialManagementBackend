namespace FinancialPlanner.Application.DTOs.Accounts;

public class AccountDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal Balance { get; set; }
    public string? Category { get; set; }
}