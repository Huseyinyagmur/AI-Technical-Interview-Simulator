using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AIInterview.API.Services;
public class PdfReportService : IPdfReportService
{
    private static readonly string Primary = "#6366F1";
    public byte[] Generate(InterviewReportDto report, string userName) => Document.Create(document => document.Page(page =>
    {
        page.Size(PageSizes.A4); page.Margin(36); page.DefaultTextStyle(x => x.FontSize(10).FontColor("#111827"));
        page.Header().Column(c => { c.Item().Text("AI Technical Interview Report").FontSize(22).Bold().FontColor(Primary); c.Item().Text("Profesyonel Mülakat Değerlendirme Raporu").FontColor("#6B7280"); c.Item().PaddingTop(8).LineHorizontal(1).LineColor("#E5E7EB"); });
        page.Content().PaddingVertical(18).Column(c =>
        {
            c.Spacing(14);
            c.Item().Row(row => { row.RelativeItem().Text($"Kullanıcı: {userName}"); row.RelativeItem().AlignRight().Text($"Oluşturulma: {DateTime.Now:dd.MM.yyyy}"); });
            c.Item().Background("#F5F7FF").Padding(16).Row(row => { row.RelativeItem().Column(x => { x.Item().Text(report.Track ?? report.Topic).Bold(); x.Item().Text($"Ulaşılan seviye: {report.Difficulty}").FontColor("#6B7280"); }); row.ConstantItem(90).AlignRight().Text($"{report.AverageScore}/100").FontSize(24).Bold().FontColor(Primary); });
            c.Item().Text("Genel değerlendirme").FontSize(14).Bold(); c.Item().Text(report.OverallFeedback);
            c.Item().Row(row => { row.RelativeItem().Element(Box).Column(x => { x.Item().Text("Güçlü alanlar").Bold(); x.Item().Text(string.Join(", ", report.StrongTopics ?? [])); }); row.RelativeItem().Element(Box).Column(x => { x.Item().Text("Gelişim alanları").Bold(); x.Item().Text(string.Join(", ", report.WeakTopics ?? [])); }); });
            c.Item().Element(Box).Column(x => { x.Item().Text("Önerilen çalışma planı").Bold(); x.Item().Text(report.StudyPlan ?? string.Join(", ", report.RecommendedTopics ?? [])); });
            c.Item().Text("Soru bazlı analiz").FontSize(14).Bold();
            foreach (var answer in report.Answers) c.Item().Border(1).BorderColor("#E5E7EB").Padding(12).Column(x => { x.Spacing(5); x.Item().Row(r => { r.RelativeItem().Text($"Soru {answer.QuestionNumber} · {answer.Concept} · {answer.Difficulty}").Bold(); r.ConstantItem(55).AlignRight().Text($"{answer.Evaluation.Score}/100").Bold().FontColor(Primary); }); x.Item().Text(answer.Question); x.Item().Text($"Cevap: {answer.Answer}").FontColor("#374151"); x.Item().Text($"Güçlü yönler: {answer.Evaluation.Strengths}"); x.Item().Text($"Eksikler: {answer.Evaluation.Weaknesses}"); x.Item().Text($"Öneri: {answer.Evaluation.ImprovementSuggestion}").FontColor("#6B7280"); });
        });
        page.Footer().AlignCenter().Text(x => { x.Span("AI Technical Interview Simulator · Sayfa "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
    })).GeneratePdf();
    private static IContainer Box(IContainer container) => container.Border(1).BorderColor("#E5E7EB").Padding(10);
}
