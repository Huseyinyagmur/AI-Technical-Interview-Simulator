using AIInterview.API.DTOs;

namespace AIInterview.API.Interfaces;

public interface IGeminiService
{
    Task<string> GenerateQuestionAsync(string topic, string difficulty, int questionNumber);
    Task<EvaluationDto> EvaluateAnswerAsync(string topic, string question, string answer);
    Task<string> GenerateFinalReportAsync(string topic, int averageScore);
}
