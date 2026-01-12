using Application.Common.Models;
using Application.DTOs.Feedback;

namespace Application.Contracts;

public interface IFeedbackService
{
    Task<Result<bool>> CreateFeedbackAsync(CreateFeedbackDto dto);
}
