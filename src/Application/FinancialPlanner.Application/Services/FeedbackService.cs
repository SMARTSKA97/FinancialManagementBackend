using AutoMapper;
using FinancialPlanner.Application.Contracts;
using FinancialPlanner.Application.DTOs.Feedback;
using FinancialPlanner.Domain.Entities;

namespace FinancialPlanner.Application.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FeedbackService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<bool>> CreateFeedbackAsync(CreateFeedbackDto dto)
    {
        var feedback = _mapper.Map<Feedback>(dto);

        // --- THIS IS THE FIX ---
        // Call the synchronous 'Upsert' method to stage the change
        _unitOfWork.FeedbackRepository.Upsert(feedback);

        // The Unit of Work saves all changes asynchronously
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.Success(true, "Feedback received. Thank you!");
    }
}