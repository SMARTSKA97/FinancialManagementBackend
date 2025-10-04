using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Feedback;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IGenericRepository<Feedback> _feedbackRepository;
    private readonly IMapper _mapper;

    public FeedbackService(IGenericRepository<Feedback> feedbackRepository, IMapper mapper)
    {
        _feedbackRepository = feedbackRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<bool>> CreateFeedbackAsync(CreateFeedbackDto dto)
    {
        var feedback = _mapper.Map<Feedback>(dto);
        await _feedbackRepository.UpsertAsync(feedback);
        return ApiResponse<bool>.Success(true, "Feedback submitted successfully.");
    }
}
