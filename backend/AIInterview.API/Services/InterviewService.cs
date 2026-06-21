using AIInterview.API.Data;
using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using AIInterview.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIInterview.API.Services;

public class InterviewService(AppDbContext db, IGeminiService gemini) : IInterviewService
{
    private const int QuestionsPerInterview = 5;
    private static readonly string[] AllowedTopics = ["C#", "OOP", "SQL", "ASP.NET Core Web API"];

    public async Task<StartInterviewResponse> StartAsync(StartInterviewRequest request)
    {
        if (!AllowedTopics.Contains(request.Topic, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Choose C#, OOP, SQL, or ASP.NET Core Web API.");
        var session = new InterviewSession { Topic = AllowedTopics.First(x => x.Equals(request.Topic, StringComparison.OrdinalIgnoreCase)), Difficulty = request.Difficulty };
        var question = new InterviewQuestion { QuestionNumber = 1, Text = await gemini.GenerateQuestionAsync(session.Topic, session.Difficulty, 1) };
        session.Questions.Add(question);
        db.InterviewSessions.Add(session);
        await db.SaveChangesAsync();
        return new StartInterviewResponse(session.Id, new QuestionDto(question.Id, question.QuestionNumber, question.Text));
    }

    public async Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request)
    {
        var session = await db.InterviewSessions.Include(x => x.Questions).ThenInclude(x => x.Answer).FirstOrDefaultAsync(x => x.Id == sessionId);
        var question = session?.Questions.SingleOrDefault(x => x.Id == request.QuestionId);
        if (session is null || question is null || question.Answer is not null || session.IsCompleted) return null;
        var evaluation = await gemini.EvaluateAnswerAsync(session.Topic, question.Text, request.Answer);
        question.Answer = new InterviewAnswer { Text = request.Answer, Evaluation = new AnswerEvaluation { Score = evaluation.Score, Strengths = evaluation.Strengths, Weaknesses = evaluation.Weaknesses, ImprovementSuggestion = evaluation.ImprovementSuggestion } };
        QuestionDto? nextQuestion = null;
        if (question.QuestionNumber < QuestionsPerInterview)
        {
            var number = question.QuestionNumber + 1;
            var next = new InterviewQuestion { InterviewSessionId = session.Id, QuestionNumber = number, Text = await gemini.GenerateQuestionAsync(session.Topic, session.Difficulty, number) };
            db.InterviewQuestions.Add(next);
            nextQuestion = new QuestionDto(0, number, next.Text); // ID is filled after SaveChanges.
            await db.SaveChangesAsync();
            nextQuestion = nextQuestion with { Id = next.Id };
        }
        else { session.IsCompleted = true; await db.SaveChangesAsync(); }
        return new SubmitAnswerResponse(evaluation, nextQuestion, session.IsCompleted);
    }

    public async Task<InterviewReportDto?> GetReportAsync(Guid sessionId)
    {
        var session = await db.InterviewSessions.Include(x => x.Questions).ThenInclude(x => x.Answer).ThenInclude(x => x!.Evaluation).FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session is null || !session.IsCompleted) return null;
        var answers = session.Questions.OrderBy(x => x.QuestionNumber).Where(x => x.Answer?.Evaluation is not null).Select(x => new ReportAnswerDto(x.QuestionNumber, x.Text, x.Answer!.Text, new EvaluationDto(x.Answer.Evaluation!.Score, x.Answer.Evaluation.Strengths, x.Answer.Evaluation.Weaknesses, x.Answer.Evaluation.ImprovementSuggestion))).ToList();
        var average = answers.Count == 0 ? 0 : (int)Math.Round(answers.Average(x => x.Evaluation.Score));
        return new InterviewReportDto(session.Id, session.Topic, session.Difficulty, average, await gemini.GenerateFinalReportAsync(session.Topic, average), answers);
    }
}
