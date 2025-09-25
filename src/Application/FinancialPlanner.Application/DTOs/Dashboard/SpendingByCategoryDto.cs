namespace FinancialPlanner.Application.DTOs.Dashboard;

public class SpendingByCategoryDto
{
    public string CategoryName { get; set; } = "Uncategorized";
    public decimal TotalAmount { get; set; }
}