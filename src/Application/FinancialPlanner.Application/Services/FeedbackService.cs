using AutoMapper;
using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Feedback;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

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