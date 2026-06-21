namespace AIInterview.API.Models;

public class InterviewAnswer
{
    public int Id { get; set; }
    public int InterviewQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public InterviewQuestion? InterviewQuestion { get; set; }
    public AnswerEvaluation? Evaluation { get; set; }
}
