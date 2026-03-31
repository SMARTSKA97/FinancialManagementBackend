using Domain.Entities;

namespace Application.DTOs.Issue;

public class IssueDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public double PainScore { get; set; }
    
    public required string CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public int Votes { get; set; }
    public int CommentCount { get; set; }
}
