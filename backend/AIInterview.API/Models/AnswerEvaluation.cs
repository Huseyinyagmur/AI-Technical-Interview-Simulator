namespace AIInterview.API.Models;

public class AnswerEvaluation
{
    public int Id { get; set; }
    public int InterviewAnswerId { get; set; }
    public int Score { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string Weaknesses { get; set; } = string.Empty;
    public string ImprovementSuggestion { get; set; } = string.Empty;
    public InterviewAnswer? InterviewAnswer { get; set; }
}
