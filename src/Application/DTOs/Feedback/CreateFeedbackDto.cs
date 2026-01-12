namespace Application.DTOs.Feedback;

public class CreateFeedbackDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
}
