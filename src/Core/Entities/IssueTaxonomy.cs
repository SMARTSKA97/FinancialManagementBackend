namespace Domain.Entities;

public class IssueTaxonomy : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = "Category"; // Category, Subcategory, Context
    
    public int? ParentId { get; set; }
    public IssueTaxonomy? Parent { get; set; }
    
    public ICollection<IssueTaxonomy> Children { get; set; } = new List<IssueTaxonomy>();
}
