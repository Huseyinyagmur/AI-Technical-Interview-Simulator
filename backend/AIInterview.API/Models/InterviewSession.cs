namespace AIInterview.API.Models;

public class InterviewSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Track { get; set; } = "General";
    public string Difficulty { get; set; } = "Junior";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public bool IsCompleted { get; set; }
    public ICollection<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();
    public User? User { get; set; }
}
