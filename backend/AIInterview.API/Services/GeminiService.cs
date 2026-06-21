using System.Text;
using System.Text.Json;
using AIInterview.API.DTOs;
using AIInterview.API.Helpers;
using AIInterview.API.Interfaces;

namespace AIInterview.API.Services;

public class GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger) : IGeminiService
{
    private readonly string _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
    private readonly string _model = configuration["Gemini:Model"] ?? "gemini-1.5-flash";

    public async Task<string> GenerateQuestionAsync(string topic, string difficulty, int questionNumber)
    {
        var template = PromptReader.Read("generate-question.txt");
        var prompt = template.Replace("{{topic}}", topic).Replace("{{difficulty}}", difficulty)
            .Replace("{{questionNumber}}", questionNumber.ToString());
        var response = await GenerateTextAsync(prompt);
        return string.IsNullOrWhiteSpace(response) ? FallbackQuestion(topic, questionNumber) : response.Trim().Trim('"');
    }

    public async Task<EvaluationDto> EvaluateAnswerAsync(string topic, string question, string answer)
    {
        var template = PromptReader.Read("evaluate-answer.txt");
        var prompt = template.Replace("{{topic}}", topic).Replace("{{question}}", question).Replace("{{answer}}", answer);
        var response = await GenerateTextAsync(prompt);
        if (TryParseEvaluation(response, out var evaluation)) return evaluation;

        // A provider hiccup should never prevent a learner from completing an interview.
        return new EvaluationDto(50, "You submitted an answer.", "The AI evaluation response could not be parsed.",
            "Try structuring your answer with a definition, an example, and relevant trade-offs.");
    }

    public async Task<string> GenerateFinalReportAsync(string topic, int averageScore)
    {
        var prompt = PromptReader.Read("final-report.txt").Replace("{{topic}}", topic).Replace("{{averageScore}}", averageScore.ToString());
        var report = await GenerateTextAsync(prompt);
        return string.IsNullOrWhiteSpace(report)
            ? $"Your average score is {averageScore}/100. Review the feedback on each answer and practise explaining {topic} concepts with concrete examples."
            : report.Trim();
    }

    private async Task<string> GenerateTextAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY") return string.Empty;
        try
        {
            var payload = JsonSerializer.Serialize(new { contents = new[] { new { parts = new[] { new { text = prompt } } } } });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini request failed; using the local fallback.");
            return string.Empty;
        }
    }

    private static bool TryParseEvaluation(string? response, out EvaluationDto evaluation)
    {
        evaluation = default!;
        if (string.IsNullOrWhiteSpace(response)) return false;
        var json = response.Trim();
        if (json.StartsWith("```")) json = json.Replace("```json", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();
        var start = json.IndexOf('{'); var end = json.LastIndexOf('}');
        if (start < 0 || end <= start) return false;
        try
        {
            using var document = JsonDocument.Parse(json[start..(end + 1)]);
            var root = document.RootElement;
            var score = root.TryGetProperty("score", out var scoreValue) && scoreValue.TryGetInt32(out var parsedScore) ? Math.Clamp(parsedScore, 0, 100) : 50;
            string Get(string name) => root.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;
            evaluation = new EvaluationDto(score, Get("strengths"), Get("weaknesses"), Get("improvementSuggestion"));
            return true;
        }
        catch (JsonException) { return false; }
    }

    private static string FallbackQuestion(string topic, int number) => number switch
    {
        1 => $"What is a core concept you should understand when working with {topic}? Explain it with an example.",
        2 => $"What common mistake can developers make with {topic}, and how would you avoid it?",
        3 => $"Describe a practical situation where you would use {topic}.",
        4 => $"What trade-off would you consider when designing a solution using {topic}?",
        _ => $"How would you explain an important {topic} concept to a junior developer?"
    };
}
