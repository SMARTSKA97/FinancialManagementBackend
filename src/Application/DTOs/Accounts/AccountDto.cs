namespace Application.DTOs.Accounts;

public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string AccountCategoryName { get; set; } = string.Empty;
}