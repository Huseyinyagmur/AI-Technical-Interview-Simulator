namespace AIInterview.API.Models;

public class InterviewSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Junior";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
    public ICollection<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();
}
