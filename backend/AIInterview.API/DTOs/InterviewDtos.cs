using System.ComponentModel.DataAnnotations;

namespace AIInterview.API.DTOs;

public class StartInterviewRequest
{
    public string Topic { get; set; } = string.Empty;
    public string? Track { get; set; }
    public string Difficulty { get; set; } = "Junior";
}
public class RegisterRequest { [Required, MaxLength(150)] public string FullName { get; set; } = string.Empty; [Required, EmailAddress] public string Email { get; set; } = string.Empty; [Required, MinLength(6)] public string Password { get; set; } = string.Empty; }
public class LoginRequest { [Required, EmailAddress] public string Email { get; set; } = string.Empty; [Required] public string Password { get; set; } = string.Empty; }
public record AuthResponse(string Token, Guid UserId, string FullName, string Email);
public class SubmitAnswerRequest
{
    [Range(1, int.MaxValue)] public int QuestionId { get; set; }
    [Required, MinLength(2)] public string Answer { get; set; } = string.Empty;
}
public record QuestionDto(int Id, int QuestionNumber, string Text, string? Concept = null, string? Difficulty = null, string? Domain = null);
public record EvaluationDto(int Score, string Strengths, string Weaknesses, string ImprovementSuggestion, string Source = "Gemini", string? ErrorMessage = null, string? RawGeminiResponse = null);
public record StartInterviewResponse(Guid SessionId, QuestionDto Question, int TotalQuestions = 5);
public record SubmitAnswerResponse(EvaluationDto Evaluation, QuestionDto? NextQuestion, bool IsCompleted);
public record ReportAnswerDto(int QuestionNumber, string Question, string Answer, EvaluationDto Evaluation, string Concept, string Difficulty);
public record TopicBreakdownDto(string Domain, int AverageScore, int QuestionCount);
public record DifficultyProgressionDto(int QuestionNumber, string Difficulty, int? Score);
public record InterviewReportDto(Guid SessionId, string Topic, string Difficulty, int AverageScore, string OverallFeedback, IReadOnlyList<ReportAnswerDto> Answers, IReadOnlyList<string>? StrongTopics = null, IReadOnlyList<string>? WeakTopics = null, IReadOnlyList<string>? RecommendedTopics = null, string? InterviewSummary = null, IReadOnlyList<TopicBreakdownDto>? TopicBreakdown = null, IReadOnlyList<DifficultyProgressionDto>? DifficultyProgression = null, string? StudyPlan = null, string? Track = null);
public record InterviewTrackDto(string Id, string Title, string Description, IReadOnlyList<string> Domains);
public record InterviewHistoryItemDto(Guid SessionId, DateTime CreatedAtUtc, string Topic, string Difficulty, int AverageScore, string Result);
public record ScoreHistoryPointDto(DateTime CompletedAtUtc, int Score, string Topic);
public record TopicPerformanceDto(string Topic, int AverageScore, int InterviewCount);
public record DashboardSummaryDto(int TotalInterviewsCompleted, int AverageInterviewScore, int HighestScore, int LowestScore, IReadOnlyList<InterviewHistoryItemDto> RecentInterviews, IReadOnlyList<ScoreHistoryPointDto> ScoreHistory, IReadOnlyList<TopicPerformanceDto> TopicPerformance);
public class DebugEvaluateRequest
{
    [Required] public string Topic { get; set; } = string.Empty;
    public string Concept { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    [Required] public string Question { get; set; } = string.Empty;
    [Required] public string Answer { get; set; } = string.Empty;
}
public record DebugEvaluateResponse(string RawGeminiResponse, string? ExtractedJson, EvaluationDto? ParsedEvaluation, string Source, string? ErrorMessage);
public record LastEvaluationDebugDto(string? RawGeminiResponse, EvaluationDto ParsedEvaluation, string Source, string? Error);
