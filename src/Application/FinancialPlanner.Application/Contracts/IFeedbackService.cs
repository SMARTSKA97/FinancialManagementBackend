using FinancialPlanner.Application.DTOs.Feedback;

namespace FinancialPlanner.Application.Contracts;

public interface IFeedbackService
{
    Task<ApiResponse<bool>> CreateFeedbackAsync(CreateFeedbackDto dto);
}
