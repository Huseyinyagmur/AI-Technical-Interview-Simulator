namespace AIInterview.API.Helpers;

// Each domain/concept has 50 deterministic prompt templates. Gemini adapts the selected template
// so sessions stay varied while retaining track and difficulty context.
public static class QuestionBank
{
    private static readonly string[] Patterns =
    [
        "Temel tanımı ve küçük bir örneği iste.", "Gerçek proje kullanım senaryosu iste.", "Yaygın hatayı ve önlemeyi sor.", "İki yaklaşımın trade-off'unu sor.",
        "Performans etkisini sor.", "Bakım kolaylığı açısından değerlendir.", "Test edilebilirlik bağlamında sor.", "Hata ayıklama senaryosu sor.",
        "Tasarım kararını gerekçelendirmesini iste.", "Kod inceleme perspektifinden sor."
    ];

    public static IReadOnlyList<string> GetTemplates(string domain, string concept, string difficulty) =>
        Enumerable.Range(0, 50).Select(index => $"{difficulty} · {domain} · {concept}: {Patterns[index % Patterns.Length]} Varyasyon {index + 1}.").ToList();

    public static string GetHints(string domain, string concept, string difficulty) => string.Join(" ", GetTemplates(domain, concept, difficulty).Take(3));
}
