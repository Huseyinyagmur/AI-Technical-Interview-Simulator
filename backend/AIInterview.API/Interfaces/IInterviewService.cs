using AIInterview.API.DTOs;

namespace AIInterview.API.Interfaces;

public interface IInterviewService
{
    Task<StartInterviewResponse> StartAsync(StartInterviewRequest request);
    Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request);
    Task<InterviewReportDto?> GetReportAsync(Guid sessionId);
}
