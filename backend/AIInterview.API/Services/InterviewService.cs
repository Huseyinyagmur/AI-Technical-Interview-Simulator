using AIInterview.API.Data;
using AIInterview.API.DTOs;
using AIInterview.API.Helpers;
using AIInterview.API.Interfaces;
using AIInterview.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIInterview.API.Services;

public class InterviewService(AppDbContext db, IGeminiService gemini) : IInterviewService
{
    private const int QuestionsPerInterview = 5;
    private static readonly string[] AllowedTopics = ["C#", "OOP", "SQL", "ASP.NET Core Web API", "Software Engineering Fundamentals", "Computer Vision"];
    private static readonly string[] DifficultyLevels = ["Junior", "Mid", "Senior"];
    private static readonly Dictionary<string, string[]> ConceptsByTopic = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OOP"] = ["Encapsulation", "Inheritance", "Polymorphism", "Abstraction", "Interfaces", "Abstract Classes", "Composition vs Inheritance", "SOLID Principles", "Design Patterns"],
        ["C#"] = ["Types", "Collections", "LINQ", "Async/Await", "Delegates", "Events", "Generics", "Exception Handling", "Memory Management", "Value Types vs Reference Types"],
        ["SQL"] = ["SELECT", "JOIN", "GROUP BY", "Indexes", "Transactions", "Normalization", "Stored Procedures", "Views", "Constraints", "Query Optimization"],
        ["ASP.NET Core Web API"] = ["Controllers", "Routing", "Dependency Injection", "Middleware", "Authentication", "Authorization", "Entity Framework Core", "REST Principles", "API Versioning", "Caching"],
        ["Software Engineering Fundamentals"] = ["Git", "GitHub", "Agile", "Scrum", "Clean Code", "Refactoring", "Unit Testing", "Integration Testing", "CI/CD", "Debugging"],
        ["Computer Vision"] = ["Image Processing", "Object Detection", "YOLO", "OCR", "CNN", "Data Annotation", "Dataset Preparation", "Precision / Recall", "mAP", "Model Deployment"]
    };

    public async Task<StartInterviewResponse> StartAsync(StartInterviewRequest request)
    {
        InterviewTrackCatalog.TrackDefinition? track = null;
        if (!string.IsNullOrWhiteSpace(request.Track) && !InterviewTrackCatalog.Tracks.TryGetValue(request.Track, out track)) throw new ArgumentException("Geçersiz interview track seçildi.");
        if (track is null && !AllowedTopics.Contains(request.Topic, StringComparer.OrdinalIgnoreCase)) throw new ArgumentException("Geçerli bir konu veya interview track seçin.");
        if (!DifficultyLevels.Contains(request.Difficulty, StringComparer.OrdinalIgnoreCase) && !request.Difficulty.Equals("Mid-level", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Choose Junior, Mid, or Senior difficulty.");
        var firstDomain = track?.Domains[0] ?? AllowedTopics.First(x => x.Equals(request.Topic, StringComparison.OrdinalIgnoreCase));
        var session = new InterviewSession { Topic = firstDomain, Track = track?.Title ?? firstDomain, Difficulty = NormalizeDifficulty(request.Difficulty) };
        var concept = SelectConcept(firstDomain, []);
        var question = new InterviewQuestion { QuestionNumber = 1, Domain = firstDomain, Concept = concept, Difficulty = session.Difficulty, Text = await gemini.GenerateQuestionAsync(firstDomain, session.Difficulty, 1, concept, [], []) };
        session.Questions.Add(question);
        db.InterviewSessions.Add(session);
        await db.SaveChangesAsync();
        return new StartInterviewResponse(session.Id, ToQuestionDto(question));
    }

    public async Task<SubmitAnswerResponse?> SubmitAnswerAsync(Guid sessionId, SubmitAnswerRequest request)
    {
        var session = await db.InterviewSessions.Include(x => x.Questions).ThenInclude(x => x.Answer).FirstOrDefaultAsync(x => x.Id == sessionId);
        var question = session?.Questions.SingleOrDefault(x => x.Id == request.QuestionId);
        if (session is null || question is null || question.Answer is not null || session.IsCompleted) return null;
        Console.WriteLine("[EVALUATION START]");
        Console.WriteLine($"[TOPIC] {session.Topic}");
        Console.WriteLine($"[CONCEPT] {question.Concept}");
        Console.WriteLine($"[DIFFICULTY] {question.Difficulty}");
        Console.WriteLine($"[QUESTION] {question.Text}");
        Console.WriteLine($"[ANSWER] {request.Answer}");
        var evaluation = await gemini.EvaluateAnswerAsync(question.Domain, question.Text, request.Answer);
        question.Answer = new InterviewAnswer { Text = request.Answer, Evaluation = new AnswerEvaluation { Score = evaluation.Score, Strengths = evaluation.Strengths, Weaknesses = evaluation.Weaknesses, ImprovementSuggestion = evaluation.ImprovementSuggestion, Source = evaluation.Source, ErrorMessage = evaluation.ErrorMessage, RawGeminiResponse = evaluation.RawGeminiResponse } };
        QuestionDto? nextQuestion = null;
        if (question.QuestionNumber < QuestionsPerInterview)
        {
            var number = question.QuestionNumber + 1;
            var scores = session.Questions.Where(x => x.Answer?.Evaluation is not null && x.Answer.Evaluation.Score >= 0).Select(x => x.Answer!.Evaluation!.Score).ToList();
            session.Difficulty = GetAdaptiveDifficulty(session.Difficulty, scores);
            var previousConcepts = session.Questions.OrderBy(x => x.QuestionNumber).Select(x => x.Concept).ToList();
            var domain = ResolveDomains(session)[(number - 1) % ResolveDomains(session).Count];
            var concept = SelectConcept(domain, session.Questions.Where(x => x.Domain == domain).Select(x => x.Concept).ToList());
            var next = new InterviewQuestion { InterviewSessionId = session.Id, QuestionNumber = number, Domain = domain, Concept = concept, Difficulty = session.Difficulty, Text = await gemini.GenerateQuestionAsync(domain, session.Difficulty, number, concept, previousConcepts, scores) };
            db.InterviewQuestions.Add(next);
            nextQuestion = ToQuestionDto(next) with { Id = 0 }; // ID is filled after SaveChanges.
            await db.SaveChangesAsync();
            nextQuestion = nextQuestion with { Id = next.Id };
        }
        else { session.IsCompleted = true; session.CompletedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(); }
        Console.WriteLine($"[FINAL SAVED EVALUATION] Score={evaluation.Score}; Source={evaluation.Source}; EvaluationId={question.Answer.Evaluation!.Id}");
        return new SubmitAnswerResponse(evaluation, nextQuestion, session.IsCompleted);
    }

    public async Task<InterviewReportDto?> GetReportAsync(Guid sessionId)
    {
        var session = await db.InterviewSessions.Include(x => x.Questions).ThenInclude(x => x.Answer).ThenInclude(x => x!.Evaluation).FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session is null || !session.IsCompleted) return null;
        var answers = session.Questions.OrderBy(x => x.QuestionNumber).Where(x => x.Answer?.Evaluation is not null).Select(x => new ReportAnswerDto(x.QuestionNumber, x.Text, x.Answer!.Text, new EvaluationDto(x.Answer.Evaluation!.Score, x.Answer.Evaluation.Strengths, x.Answer.Evaluation.Weaknesses, x.Answer.Evaluation.ImprovementSuggestion, x.Answer.Evaluation.Source, x.Answer.Evaluation.ErrorMessage, x.Answer.Evaluation.RawGeminiResponse), x.Concept, x.Difficulty)).ToList();
        var scoredAnswers = answers.Where(x => x.Evaluation.Score >= 0).ToList();
        var average = scoredAnswers.Count == 0 ? 0 : (int)Math.Round(scoredAnswers.Average(x => x.Evaluation.Score));
        var topicScores = scoredAnswers.GroupBy(x => x.Concept).Select(x => new { Concept = x.Key, Score = x.Average(y => y.Evaluation.Score) }).ToList();
        var strong = topicScores.Where(x => x.Score >= 75).OrderByDescending(x => x.Score).Select(x => x.Concept).ToList();
        var weak = topicScores.Where(x => x.Score < 70).OrderBy(x => x.Score).Select(x => x.Concept).ToList();
        var recommended = weak.Any() ? weak : topicScores.OrderBy(x => x.Score).Take(2).Select(x => x.Concept).ToList();
        var breakdown = scoredAnswers.GroupBy(x => x.Concept).Select(x => new TopicBreakdownDto(x.Key, (int)Math.Round(x.Average(y => y.Evaluation.Score)), x.Count())).ToList();
        var progression = session.Questions.OrderBy(x => x.QuestionNumber).Select(x => new DifficultyProgressionDto(x.QuestionNumber, x.Difficulty, x.Answer?.Evaluation?.Score >= 0 ? x.Answer.Evaluation.Score : null)).ToList();
        var roadmap = recommended.Count == 0 ? "Mevcut güçlü alanlarınızı gerçek proje senaryolarıyla pekiştirin." : $"Önce {recommended[0]} konusunu çalışın. Ardından {string.Join(" ve ", recommended.Skip(1))} ile pratik yapın.";
        var summary = $"{session.Track} görüşmesinde {answers.Count} soruyu tamamladınız. Son ulaşılan zorluk seviyesi: {session.Difficulty}.";
        return new InterviewReportDto(session.Id, session.Topic, session.Difficulty, average, await gemini.GenerateFinalReportAsync(session.Track, average), answers, strong, weak, recommended, summary, breakdown, progression, roadmap, session.Track);
    }

    public async Task<IReadOnlyList<InterviewHistoryItemDto>> GetHistoryAsync() => await db.InterviewSessions.Where(x => x.IsCompleted).Include(x => x.Questions).ThenInclude(x => x.Answer).ThenInclude(x => x!.Evaluation).OrderByDescending(x => x.CompletedAtUtc).Select(x => new InterviewHistoryItemDto(x.Id, x.CreatedAtUtc, x.Topic, x.Difficulty, (int)Math.Round(x.Questions.Where(q => q.Answer!.Evaluation != null).Average(q => q.Answer!.Evaluation!.Score)), "Tamamlandı")).ToListAsync();

    public async Task<DashboardSummaryDto> GetDashboardAsync()
    {
        var history = await GetHistoryAsync();
        var sessions = await db.InterviewSessions.Where(x => x.IsCompleted).Include(x => x.Questions).ThenInclude(x => x.Answer).ThenInclude(x => x!.Evaluation).ToListAsync();
        var scoreHistory = history.OrderBy(x => x.CreatedAtUtc).Select(x => new ScoreHistoryPointDto(x.CreatedAtUtc, x.AverageScore, x.Topic)).ToList();
        var performance = sessions.GroupBy(x => x.Topic).Select(x => new TopicPerformanceDto(x.Key, (int)Math.Round(x.SelectMany(s => s.Questions).Where(q => q.Answer?.Evaluation != null).Average(q => q.Answer!.Evaluation!.Score)), x.Count())).ToList();
        var scores = history.Select(x => x.AverageScore).ToList();
        return new DashboardSummaryDto(history.Count, scores.Count == 0 ? 0 : (int)Math.Round(scores.Average()), scores.Count == 0 ? 0 : scores.Max(), scores.Count == 0 ? 0 : scores.Min(), history.Take(5).ToList(), scoreHistory, performance);
    }

    private static QuestionDto ToQuestionDto(InterviewQuestion question) => new(question.Id, question.QuestionNumber, question.Text, question.Concept, question.Difficulty, question.Domain);
    private static string NormalizeDifficulty(string difficulty) => difficulty.Equals("Mid-level", StringComparison.OrdinalIgnoreCase) ? "Mid" : DifficultyLevels.First(x => x.Equals(difficulty, StringComparison.OrdinalIgnoreCase));
    private static string GetAdaptiveDifficulty(string current, IReadOnlyList<int> scores)
    {
        var index = Array.IndexOf(DifficultyLevels, NormalizeDifficulty(current));
        var average = scores.Count == 0 ? 0 : scores.Average();
        return average >= 90 ? DifficultyLevels[Math.Min(index + 1, DifficultyLevels.Length - 1)] : average < 70 ? DifficultyLevels[Math.Max(index - 1, 0)] : DifficultyLevels[index];
    }
    private static string SelectConcept(string topic, IReadOnlyList<string> history)
    {
        var concepts = ConceptsByTopic[topic];
        var unused = concepts.FirstOrDefault(x => !history.Contains(x, StringComparer.OrdinalIgnoreCase));
        return unused ?? concepts.First(x => !x.Equals(history.LastOrDefault(), StringComparison.OrdinalIgnoreCase));
    }
    private static IReadOnlyList<string> ResolveDomains(InterviewSession session) => InterviewTrackCatalog.Tracks.Values.FirstOrDefault(x => x.Title.Equals(session.Track, StringComparison.OrdinalIgnoreCase))?.Domains ?? [session.Topic];
}
