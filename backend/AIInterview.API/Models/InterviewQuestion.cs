namespace AIInterview.API.Models;

public class InterviewQuestion
{
    public int Id { get; set; }
    public Guid InterviewSessionId { get; set; }
    public int QuestionNumber { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Junior";
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public InterviewSession? InterviewSession { get; set; }
    public InterviewAnswer? Answer { get; set; }
}
