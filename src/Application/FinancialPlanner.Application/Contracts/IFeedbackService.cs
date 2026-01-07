using FinancialPlanner.Application.Common.Models;
using FinancialPlanner.Application.DTOs.Feedback;

namespace FinancialPlanner.Application.Contracts;

public interface IFeedbackService
{
    Task<Result<bool>> CreateFeedbackAsync(CreateFeedbackDto dto);
}
