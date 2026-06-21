using AIInterview.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIInterview.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<InterviewSession> InterviewSessions => Set<InterviewSession>();
    public DbSet<InterviewQuestion> InterviewQuestions => Set<InterviewQuestion>();
    public DbSet<InterviewAnswer> InterviewAnswers => Set<InterviewAnswer>();
    public DbSet<AnswerEvaluation> AnswerEvaluations => Set<AnswerEvaluation>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InterviewSession>().Property(x => x.Topic).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<InterviewSession>().Property(x => x.Difficulty).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<InterviewSession>().Property(x => x.Track).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.FullName).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(256).IsRequired();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<InterviewQuestion>().Property(x => x.Text).IsRequired();
        modelBuilder.Entity<InterviewQuestion>().Property(x => x.Concept).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<InterviewQuestion>().Property(x => x.Domain).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<InterviewQuestion>().Property(x => x.Difficulty).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<InterviewAnswer>().Property(x => x.Text).IsRequired();
        modelBuilder.Entity<AnswerEvaluation>().Property(x => x.Source).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<InterviewSession>()
            .HasMany(x => x.Questions).WithOne(x => x.InterviewSession)
            .HasForeignKey(x => x.InterviewSessionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>().HasMany(x => x.InterviewSessions).WithOne(x => x.User)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<InterviewQuestion>()
            .HasOne(x => x.Answer).WithOne(x => x.InterviewQuestion)
            .HasForeignKey<InterviewAnswer>(x => x.InterviewQuestionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<InterviewAnswer>()
            .HasOne(x => x.Evaluation).WithOne(x => x.InterviewAnswer)
            .HasForeignKey<AnswerEvaluation>(x => x.InterviewAnswerId).OnDelete(DeleteBehavior.Cascade);
    }
}
