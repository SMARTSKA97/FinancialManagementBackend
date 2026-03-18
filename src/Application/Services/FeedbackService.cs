
using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Feedback;
using Domain.Entities;

namespace Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IApplicationDbContext _context;
    public FeedbackService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> CreateFeedbackAsync(CreateFeedbackDto dto)
    {
        var feedback = new Feedback
        {
            Name = dto.Name,
            Email = dto.Email,
            Type = dto.Type,
            Description = dto.Description
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }
}