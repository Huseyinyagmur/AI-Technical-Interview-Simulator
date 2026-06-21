using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIInterview.API.DTOs;
using AIInterview.API.Helpers;
using AIInterview.API.Interfaces;

namespace AIInterview.API.Services;

public class GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger) : IGeminiService
{
    private readonly string _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? configuration["Gemini:ApiKey"] ?? string.Empty;
    private readonly string _model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? configuration["Gemini:Model"] ?? "gemini-2.5-flash";

    public async Task<string> GenerateQuestionAsync(string topic, string difficulty, int questionNumber, string concept, IReadOnlyList<string> previousConcepts, IReadOnlyList<int> previousScores)
    {
        var template = PromptReader.Read("generate-question.txt");
        var prompt = template.Replace("{{topic}}", topic).Replace("{{difficulty}}", difficulty)
            .Replace("{{questionNumber}}", questionNumber.ToString())
            .Replace("{{concept}}", concept)
            .Replace("{{previousConcepts}}", previousConcepts.Count == 0 ? "Yok" : string.Join(", ", previousConcepts))
            .Replace("{{previousScores}}", previousScores.Count == 0 ? "Yok" : string.Join(", ", previousScores));
        var response = await GenerateTextAsync(prompt);
        return string.IsNullOrWhiteSpace(response) ? FallbackQuestion(topic, questionNumber) : response.Trim().Trim('"');
    }

    public async Task<EvaluationDto> EvaluateAnswerAsync(string topic, string question, string answer)
    {
        var template = PromptReader.Read("evaluate-answer.txt");
        var prompt = template.Replace("{{topic}}", topic).Replace("{{question}}", question).Replace("{{answer}}", answer)
            .Replace("{{questionType}}", DetermineQuestionType(question));
        Console.WriteLine("[GEMINI EVALUATION REQUEST]");
        Console.WriteLine($"[GEMINI REQUEST PROMPT] {prompt}");
        logger.LogInformation("[GEMINI EVALUATION REQUEST] QuestionType={QuestionType}; Prompt={Prompt}", DetermineQuestionType(question), prompt);
        var response = await GenerateTextDetailsAsync(prompt, expectJson: true);
        // Log before any cleanup or extraction: this is the exact text returned by the model.
        Console.WriteLine($"[GEMINI RAW RESPONSE] {(!string.IsNullOrWhiteSpace(response.Text) ? response.Text : response.RawApiResponse)}");
        logger.LogInformation("[GEMINI EVALUATION RESPONSE] {GeminiResponse}", response.Text);
        logger.LogInformation("[GEMINI RAW RESPONSE] {GeminiResponse}", response.Text);
        if (string.IsNullOrWhiteSpace(response.Text))
        {
            var source = response.FailureSource ?? (string.IsNullOrWhiteSpace(response.Error) ? "MissingEvaluation" : "Fallback");
            Console.WriteLine($"[FALLBACK REASON] Source={source}; Reason={response.Error ?? "empty Gemini response"}; RawApiResponse={response.RawApiResponse}");
            logger.LogError("[FALLBACK REASON] Source={Source}; Reason={Reason}; RawApiResponse={RawApiResponse}", source, response.Error ?? "empty Gemini response", response.RawApiResponse);
            logger.LogWarning("[EVALUATION SOURCE] {Source}. {Error}", source, response.Error ?? "empty Gemini response");
            return CreateFailureEvaluation(source, response.Error ?? "Evaluation failed: empty Gemini response", string.IsNullOrWhiteSpace(response.Text) ? response.RawApiResponse : response.Text);
        }
        if (TryParseEvaluation(response.Text, out var evaluation, out _, out var parseError))
        {
            logger.LogInformation("[EVALUATION SOURCE] Gemini");
            return evaluation with { RawGeminiResponse = response.Text };
        }

        // A provider hiccup should never prevent a learner from completing an interview.
        Console.WriteLine($"[FALLBACK REASON] Source=ParseFailed; Reason={parseError ?? "Evaluation failed: parse error"}; RawResponse={response.Text}");
        logger.LogWarning("[EVALUATION SOURCE] ParseFailed");
        return CreateFailureEvaluation("ParseFailed", parseError is null ? "Evaluation failed: parse error" : $"Evaluation failed: parse error: {parseError}", response.Text);
    }

    public async Task<DebugEvaluateResponse> DebugEvaluateAnswerAsync(string topic, string concept, string difficulty, string question, string answer)
    {
        var template = PromptReader.Read("evaluate-answer.txt");
        var prompt = template.Replace("{{topic}}", topic).Replace("{{question}}", question).Replace("{{answer}}", answer)
            .Replace("{{questionType}}", DetermineQuestionType(question));
        Console.WriteLine("[GEMINI EVALUATION REQUEST]");
        Console.WriteLine($"[GEMINI REQUEST PROMPT] {prompt}");
        logger.LogInformation("[GEMINI EVALUATION REQUEST] QuestionType={QuestionType}; Prompt={Prompt}", DetermineQuestionType(question), prompt);
        var response = await GenerateTextDetailsAsync(prompt, expectJson: true);
        Console.WriteLine($"[GEMINI RAW RESPONSE] {(!string.IsNullOrWhiteSpace(response.Text) ? response.Text : response.RawApiResponse)}");
        logger.LogInformation("[GEMINI EVALUATION RESPONSE] {GeminiResponse}", response.Text);
        logger.LogInformation("[GEMINI RAW RESPONSE] {GeminiResponse}", response.Text);
        var parsed = TryParseEvaluation(response.Text, out var evaluation, out var extractedJson, out var parseError);
        var source = parsed ? "Gemini" : response.FailureSource ?? (!string.IsNullOrWhiteSpace(response.Error) ? "Fallback" : string.IsNullOrWhiteSpace(response.Text) ? "MissingEvaluation" : "ParseFailed");
        return new DebugEvaluateResponse(string.IsNullOrWhiteSpace(response.Text) ? response.RawApiResponse : response.Text, extractedJson, parsed ? evaluation : null, source, response.Error ?? parseError);
    }

    public async Task<string> GenerateFinalReportAsync(string topic, int averageScore)
    {
        var prompt = PromptReader.Read("final-report.txt").Replace("{{topic}}", topic).Replace("{{averageScore}}", averageScore.ToString());
        var report = await GenerateTextAsync(prompt);
        return string.IsNullOrWhiteSpace(report)
            ? $"Ortalama puanınız {averageScore}/100. Her cevaptaki geri bildirimi inceleyin ve {topic} kavramlarını somut örneklerle açıklama pratiği yapın."
            : report.Trim();
    }

    private async Task<string> GenerateTextAsync(string prompt) => (await GenerateTextDetailsAsync(prompt)).Text;

    private async Task<GeminiTextResponse> GenerateTextDetailsAsync(string prompt, bool expectJson = false)
    {
        Console.WriteLine($"[GEMINI API KEY EXISTS] {!string.IsNullOrWhiteSpace(_apiKey) && _apiKey != "YOUR_GEMINI_API_KEY"}");
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
        Console.WriteLine($"[GEMINI MODEL] {_model}");
        Console.WriteLine($"[GEMINI ENDPOINT] {endpoint}");
        // The key is intentionally omitted from logs even in Development.
        Console.WriteLine($"[FULL REQUEST URL] {endpoint}?key=[REDACTED]");
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY")
        {
            logger.LogWarning("Gemini API anahtarı yapılandırılmamış; yerel yedek yanıt kullanılacak.");
            return new GeminiTextResponse(string.Empty, string.Empty, "Evaluation failed: Gemini API key is missing");
        }
        try
        {
            object requestBody = expectJson
                ? new { contents = new[] { new { parts = new[] { new { text = prompt } } } }, generationConfig = new { responseMimeType = "application/json" } }
                : new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var payload = JsonSerializer.Serialize(requestBody);
            for (var attempt = 0; attempt <= 3; attempt++)
            {
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync($"{endpoint}?key={_apiKey}", content);
                var rawApiResponse = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 429)
                {
                    Console.WriteLine($"[RATE LIMIT DETECTED] HTTP 429; Body={rawApiResponse}");
                    logger.LogWarning("[RATE LIMIT DETECTED] HTTP 429; Body={ResponseBody}", rawApiResponse);
                    if (attempt < 3)
                    {
                        var delaySeconds = 1 << attempt;
                        Console.WriteLine($"[RETRY ATTEMPT] {attempt + 1}/3; waiting {delaySeconds}s");
                        logger.LogInformation("[RETRY ATTEMPT] {Attempt}/3; waiting {DelaySeconds}s", attempt + 1, delaySeconds);
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        continue;
                    }
                    Console.WriteLine("[RETRY FAILED] Gemini remained rate limited after 3 retries.");
                    logger.LogError("[RETRY FAILED] Gemini remained rate limited after 3 retries.");
                    return new GeminiTextResponse(string.Empty, rawApiResponse, "Gemini API rate limitine ulaşıldı. Birkaç dakika sonra tekrar deneyin.", "RateLimited");
                }
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("[GEMINI RAW RESPONSE] HTTP {StatusCode}: {ResponseBody}", response.StatusCode, rawApiResponse);
                    logger.LogError("Gemini API yanıtı başarısız. Durum kodu: {StatusCode}. Gövde: {ResponseBody}", response.StatusCode, rawApiResponse);
                    return new GeminiTextResponse(string.Empty, rawApiResponse, $"Evaluation failed: Gemini API returned {(int)response.StatusCode}");
                }

                logger.LogInformation("[GEMINI RAW RESPONSE] Gemini API HTTP gövdesi: {RawApiResponse}", rawApiResponse);
                try
                {
                    using var document = JsonDocument.Parse(rawApiResponse);
                    var text = document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? string.Empty;
                    return new GeminiTextResponse(text, rawApiResponse, null);
                }
                catch (Exception ex) when (ex is JsonException or KeyNotFoundException or IndexOutOfRangeException)
                {
                    logger.LogError(ex, "Gemini API yanıt gövdesi beklenen biçimde değil. Ham gövde: {RawApiResponse}", rawApiResponse);
                    return new GeminiTextResponse(string.Empty, rawApiResponse, "Evaluation failed: unexpected Gemini response structure");
                }
            }
            throw new InvalidOperationException("Retry loop unexpectedly completed.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Gemini API isteği başarısız oldu; yerel yedek yanıt kullanılacak.");
            return new GeminiTextResponse(string.Empty, string.Empty, "Evaluation failed: Gemini API request failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Beklenmeyen Gemini isteği hatası; yerel yedek yanıt kullanılacak.");
            return new GeminiTextResponse(string.Empty, string.Empty, "Evaluation failed: unexpected Gemini error");
        }
    }

    private bool TryParseEvaluation(string? response, out EvaluationDto evaluation, out string? extractedJson, out string? parseError)
    {
        evaluation = default!;
        extractedJson = null;
        parseError = null;
        if (string.IsNullOrWhiteSpace(response))
        {
            parseError = "Gemini değerlendirme yanıtı boş.";
            Console.WriteLine($"[GEMINI PARSE ERROR] {parseError}");
            logger.LogError("[GEMINI PARSE ERROR] {ParseError}", parseError);
            return false;
        }

        // Remove common Markdown fences, then locate one balanced JSON object even if prose surrounds it.
        var cleaned = Regex.Replace(response, @"```(?:json)?", string.Empty, RegexOptions.IgnoreCase).Trim();
        cleaned = TryDecodeEscapedJson(cleaned);
        if (!TryExtractJsonObject(cleaned, out var json))
        {
            parseError = "Yanıtta dengeli bir JSON nesnesi bulunamadı.";
            Console.WriteLine($"[GEMINI PARSE ERROR] {parseError}; Raw={response}");
            logger.LogError("[GEMINI PARSE ERROR] {ParseError}. Ham yanıt: {RawResponse}", parseError, response);
            return false;
        }

        extractedJson = json;
        Console.WriteLine($"[GEMINI EXTRACTED JSON] {json}");
        logger.LogInformation("[PARSED JSON] {EvaluationJson}", json);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) throw new JsonException("Kök JSON değeri nesne değil.");
            if (!TryGetScore(root, out var score)) throw new JsonException("score alanı 0-100 arasında sayı veya sayı metni olmalıdır.");
            string Get(string name) => root.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;
            evaluation = new EvaluationDto(score, Get("strengths"), Get("weaknesses"), Get("improvementSuggestion"), "Gemini");
            return true;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            parseError = ex.Message;
            Console.WriteLine($"[GEMINI PARSE ERROR] {parseError}; Raw={response}; Json={json}");
            logger.LogError(ex, "[GEMINI PARSE ERROR] {ParseError}. Ham yanıt: {RawResponse}. Çıkarılan JSON: {EvaluationJson}", parseError, response, json);
            return false;
        }
    }

    private static bool TryGetScore(JsonElement root, out int score)
    {
        score = 0;
        if (!root.TryGetProperty("score", out var value)) return false;
        if (value.TryGetInt32(out var numericScore)) { score = Math.Clamp(numericScore, 0, 100); return true; }
        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var stringScore)) { score = Math.Clamp(stringScore, 0, 100); return true; }
        return false;
    }

    private static EvaluationDto CreateFailureEvaluation(string source, string message, string? rawResponse) =>
        new(source == "RateLimited" ? -1 : 0, string.Empty, message, source == "RateLimited" ? "AI değerlendirmesi geçici olarak kullanılamıyor." : "Gemini yapılandırmasını ve uygulama loglarını kontrol edip cevabı yeniden gönderin.", source, message, rawResponse);

    private static EvaluationDto CreateRuleBasedFallback(string question, string answer)
    {
        var type = DetermineQuestionType(question);
        var words = answer.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var score = words switch { < 8 => 30, < 25 => 45, < 60 => 60, _ => 70 };
        var suggestion = type switch
        {
            "SQL_CODE_REQUIRED" => "Sorunun istediği SQL ifadesini geçerli sözdizimiyle yazın ve seçiminizi kısaca açıklayın.",
            "EXPLANATION" => "Seçtiğiniz kavramı basit bir dille, hedef kitleye uygun somut bir örnekle açıklayın.",
            _ when question.Contains("avantaj", StringComparison.OrdinalIgnoreCase) || question.Contains("dezavantaj", StringComparison.OrdinalIgnoreCase) => "Sorunun istediği avantaj ve dezavantajları ayrı, somut maddelerle ele alın.",
            _ => "Sorunun doğrudan istediği noktayı yanıtlayın; gerekirse kısa bir örnek ve gerekçenizi ekleyin."
        };
        return new EvaluationDto(score, "Yanıtınız değerlendirme için kaydedildi.", "Gemini değerlendirmesi şu anda kullanılamadı; bu puan temel yanıt uzunluğu kuralına dayanır.", suggestion, "RuleBased");
    }

    private static string DetermineQuestionType(string question)
    {
        var normalized = question.ToLowerInvariant();
        var asksForSqlCode = (normalized.Contains("sql") || normalized.Contains("sorgu") || normalized.Contains("query")) &&
                             (normalized.Contains("yaz") || normalized.Contains("oluştur") || normalized.Contains("kod") || normalized.Contains("statement") || normalized.Contains("query"));
        if (asksForSqlCode) return "SQL_CODE_REQUIRED";
        if (normalized.Contains("açıkla") || normalized.Contains("anlat") || normalized.Contains("tanımla") || normalized.Contains("describe") || normalized.Contains("explain") || normalized.Contains("junior")) return "EXPLANATION";
        return "GENERAL";
    }

    private static string TryDecodeEscapedJson(string text)
    {
        if (!text.StartsWith('"') || !text.EndsWith('"')) return text;
        try { return JsonSerializer.Deserialize<string>(text) ?? text; }
        catch (JsonException) { return text; }
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

    private sealed record GeminiTextResponse(string Text, string RawApiResponse, string? Error, string? FailureSource = null);
}
