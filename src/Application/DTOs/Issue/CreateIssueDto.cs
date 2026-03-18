namespace Application.DTOs.Issue;

public class CreateIssueDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string Priority { get; set; } = "Medium";
    
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    
    public string Severity { get; set; } = "Minor";
    public bool ImpactsMoney { get; set; }
    public decimal? FinancialImpactAmount { get; set; }
    public string Frequency { get; set; } = "Rare";
    
    // Initial comment or metadata?
    public string? StructuredExpecations { get; set; }
}
