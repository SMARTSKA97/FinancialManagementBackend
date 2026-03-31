namespace Application.DTOs.Issue;

public class CreateCommentDto
{
    public required string Content { get; set; }
    public string? StructuredMetadata { get; set; }
}
