using System.ComponentModel.DataAnnotations;

namespace AIInterview.API.DTOs;

public class StartInterviewRequest
{
    [Required] public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Junior";
}
public class SubmitAnswerRequest
{
    [Range(1, int.MaxValue)] public int QuestionId { get; set; }
    [Required, MinLength(2)] public string Answer { get; set; } = string.Empty;
}
public record QuestionDto(int Id, int QuestionNumber, string Text);
public record EvaluationDto(int Score, string Strengths, string Weaknesses, string ImprovementSuggestion);
public record StartInterviewResponse(Guid SessionId, QuestionDto Question, int TotalQuestions = 5);
public record SubmitAnswerResponse(EvaluationDto Evaluation, QuestionDto? NextQuestion, bool IsCompleted);
public record ReportAnswerDto(int QuestionNumber, string Question, string Answer, EvaluationDto Evaluation);
public record InterviewReportDto(Guid SessionId, string Topic, string Difficulty, int AverageScore, string OverallFeedback, IReadOnlyList<ReportAnswerDto> Answers);
