using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        // This is intentionally logged before parsing so malformed Gemini output is diagnosable.
        logger.LogInformation("Ham Gemini değerlendirme yanıtı: {GeminiResponse}", response);
        if (TryParseEvaluation(response, out var evaluation)) return evaluation;

        // A provider hiccup should never prevent a learner from completing an interview.
        return new EvaluationDto(50, "Cevabınızı gönderdiniz.", "Yapay zekâ değerlendirme yanıtı ayrıştırılamadı.",
            "Cevabınızı tanım, örnek ve ilgili avantaj/dezavantajları içerecek şekilde yapılandırmayı deneyin.");
    }

    public async Task<string> GenerateFinalReportAsync(string topic, int averageScore)
    {
        var prompt = PromptReader.Read("final-report.txt").Replace("{{topic}}", topic).Replace("{{averageScore}}", averageScore.ToString());
        var report = await GenerateTextAsync(prompt);
        return string.IsNullOrWhiteSpace(report)
            ? $"Ortalama puanınız {averageScore}/100. Her cevaptaki geri bildirimi inceleyin ve {topic} kavramlarını somut örneklerle açıklama pratiği yapın."
            : report.Trim();
    }

    private async Task<string> GenerateTextAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
        {
            logger.LogWarning("Gemini API anahtarı yapılandırılmamış; yerel yedek yanıt kullanılacak.");
            return string.Empty;
        }
        try
        {
            var payload = JsonSerializer.Serialize(new { contents = new[] { new { parts = new[] { new { text = prompt } } } } });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}", content);
            var rawApiResponse = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Gemini API yanıtı başarısız. Durum kodu: {StatusCode}. Gövde: {ResponseBody}", response.StatusCode, rawApiResponse);
                return string.Empty;
            }

            logger.LogInformation("Ham Gemini API yanıtı: {RawApiResponse}", rawApiResponse);
            try
            {
                using var document = JsonDocument.Parse(rawApiResponse);
                var text = document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
                logger.LogInformation("Gemini metin yanıtı: {GeminiText}", text);
                return text;
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IndexOutOfRangeException)
            {
                logger.LogError(ex, "Gemini API yanıt gövdesi beklenen biçimde değil. Ham gövde: {RawApiResponse}", rawApiResponse);
                return string.Empty;
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Gemini API isteği başarısız oldu; yerel yedek yanıt kullanılacak.");
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Beklenmeyen Gemini isteği hatası; yerel yedek yanıt kullanılacak.");
            return string.Empty;
        }
    }

    private bool TryParseEvaluation(string? response, out EvaluationDto evaluation)
    {
        evaluation = default!;
        if (string.IsNullOrWhiteSpace(response))
        {
            logger.LogError("Gemini değerlendirme yanıtı boş olduğu için JSON ayrıştırması yapılamadı.");
            return false;
        }

        // Remove common Markdown fences, then locate one balanced JSON object even if prose surrounds it.
        var cleaned = Regex.Replace(response, @"```(?:json)?", string.Empty, RegexOptions.IgnoreCase).Trim();
        if (!TryExtractJsonObject(cleaned, out var json))
        {
            logger.LogError("Gemini değerlendirme yanıtında JSON nesnesi bulunamadı. Ham yanıt: {RawResponse}", response);
            return false;
        }

        logger.LogInformation("Ayrıştırılacak Gemini değerlendirme JSON'u: {EvaluationJson}", json);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new JsonException("Kök JSON değeri nesne değil.");
            var score = root.TryGetProperty("score", out var scoreValue) && scoreValue.TryGetInt32(out var parsedScore) ? Math.Clamp(parsedScore, 0, 100) : 50;
            string Get(string name) => root.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;
            evaluation = new EvaluationDto(score, Get("strengths"), Get("weaknesses"), Get("improvementSuggestion"));
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            logger.LogError(ex, "Gemini değerlendirme JSON'u ayrıştırılamadı. Ham yanıt: {RawResponse}. Çıkarılan JSON: {EvaluationJson}", response, json);
            return false;
        }
    }

    private static bool TryExtractJsonObject(string text, out string json)
    {
        json = string.Empty;
        var start = text.IndexOf('{');
        if (start < 0) return false;
        var depth = 0;
        var insideString = false;
        var escaped = false;
        for (var index = start; index < text.Length; index++)
        {
            var character = text[index];
            if (insideString)
            {
                if (escaped) escaped = false;
                else if (character == '\\') escaped = true;
                else if (character == '"') insideString = false;
                continue;
            }
            if (character == '"') insideString = true;
            else if (character == '{') depth++;
            else if (character == '}' && --depth == 0)
            {
                json = text[start..(index + 1)];
                return true;
            }
        }
        return false;
    }

    private static string FallbackQuestion(string topic, int number) => number switch
    {
        1 => $"{topic} ile çalışırken bilmeniz gereken temel bir kavram nedir? Bir örnekle açıklayın.",
        2 => $"Geliştiriciler {topic} ile çalışırken hangi yaygın hatayı yapabilir ve bunu nasıl önlersiniz?",
        3 => $"{topic} kullanacağınız pratik bir durumu açıklayın.",
        4 => $"{topic} kullanan bir çözüm tasarlarken hangi avantaj/dezavantajı değerlendirirsiniz?",
        _ => $"Önemli bir {topic} kavramını junior bir geliştiriciye nasıl açıklarsınız?"
    };
}
