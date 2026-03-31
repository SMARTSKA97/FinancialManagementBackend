using Application.Common.Models;

namespace Application.DTOs.Transactions;

public class TransactionPageResultDto
{
    public PaginatedResult<TransactionDto> Transactions { get; set; } = null!;
    public decimal BalanceBroughtForward { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSavings { get; set; }
    public List<string> AvailableCategories { get; set; } = new();
}
