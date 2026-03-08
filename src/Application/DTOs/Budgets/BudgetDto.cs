using Domain.Enums;

namespace Application.DTOs.Budgets;

public class BudgetDto
{
    public int Id { get; set; }
    public int? TransactionCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
