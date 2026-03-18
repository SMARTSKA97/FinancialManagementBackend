namespace Domain.Entities;

public class Issue : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    
    // Status & Lifecycle
    public string Status { get; set; } = "New"; // New, Acknowledged, Triaged, Planned, InProgress, Released, Verified
    public string Priority { get; set; } = "Medium"; // Low, Medium, High
    
    // Taxonomy Links
    public int? CategoryId { get; set; }
    public IssueTaxonomy? Category { get; set; }
    
    public int? SubcategoryId { get; set; }
    public IssueTaxonomy? Subcategory { get; set; }

    // Ranking Logic Inputs
    public string Severity { get; set; } = "Minor"; // Minor, Major, Critical
    public bool ImpactsMoney { get; set; }
    public decimal? FinancialImpactAmount { get; set; }
    public string Frequency { get; set; } = "Rare"; // Rare, Frequent, Always
    
    public int TrustPenalty { get; set; } = 0;
    public int Votes { get; set; } = 0;
    
    public double PainScore { get; set; } = 0; // Computed ranking score

    // User Info
    public string? CreatorUserId { get; set; }
    
    // Collections
    public ICollection<IssueComment> Comments { get; set; } = new List<IssueComment>();
}
