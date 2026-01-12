namespace Domain.Entities;

public class Feedback : BaseEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Type { get; set; } // e.g., "bug", "feature"
    public required string Description { get; set; }
}