using AIInterview.API.DTOs;

namespace AIInterview.API.Interfaces;

public interface IInterviewService
{
    Task<StartInterviewResponse> StartAsync(Guid userId, StartInterviewRequest request);
    Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid userId, Guid sessionId, SubmitAnswerRequest request);
    Task<InterviewReportDto?> GetReportAsync(Guid userId, Guid sessionId);
    Task<IReadOnlyList<InterviewHistoryItemDto>> GetHistoryAsync(Guid userId);
    Task<DashboardSummaryDto> GetDashboardAsync(Guid userId);
}
