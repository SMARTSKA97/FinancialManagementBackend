using AutoMapper;
using Application.Common.Models;
using Application.Contracts;
using Application.DTOs.Feedback;
using Domain.Entities;

namespace Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public FeedbackService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<bool>> CreateFeedbackAsync(CreateFeedbackDto dto)
    {
        var feedback = _mapper.Map<Feedback>(dto);

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return Result.Success(true);
    }
}