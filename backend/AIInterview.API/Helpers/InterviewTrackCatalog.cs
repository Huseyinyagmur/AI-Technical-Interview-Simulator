using AIInterview.API.DTOs;

namespace AIInterview.API.Helpers;

public static class InterviewTrackCatalog
{
    public static readonly IReadOnlyDictionary<string, TrackDefinition> Tracks = new Dictionary<string, TrackDefinition>(StringComparer.OrdinalIgnoreCase)
    {
        ["csharp-backend"] = new("csharp-backend", "C# Backend Developer", "C#, OOP, LINQ, EF Core, Web API ve SQL odaklı teknik mülakat.", ["OOP", "C#", "SQL", "ASP.NET Core Web API"]),
        ["aspnet-core"] = new("aspnet-core", "ASP.NET Core Developer", "Web API, middleware, DI, EF Core, güvenlik ve REST odaklı görüşme.", ["ASP.NET Core Web API", "C#", "SQL"]),
        ["sql-developer"] = new("sql-developer", "SQL Developer", "Sorgular, performans, transaction ve veri modelleme odaklı görüşme.", ["SQL"]),
        ["software-fundamentals"] = new("software-fundamentals", "Software Engineering Fundamentals", "Git, test, Clean Code, Agile ve CI/CD temelleri.", ["Software Engineering Fundamentals"]),
        ["computer-vision"] = new("computer-vision", "Computer Vision Engineer", "Görüntü işleme, YOLO, CNN, OCR, metrikler ve deployment.", ["Computer Vision"]),
        ["mixed"] = new("mixed", "Mixed Technical Interview", "Birden fazla teknik alandan dengeli karma görüşme.", ["OOP", "C#", "SQL", "ASP.NET Core Web API", "Software Engineering Fundamentals", "Computer Vision"])
    };

    public static IReadOnlyList<InterviewTrackDto> List() => Tracks.Values.Select(x => new InterviewTrackDto(x.Id, x.Title, x.Description, x.Domains)).ToList();
    public sealed record TrackDefinition(string Id, string Title, string Description, IReadOnlyList<string> Domains);
}
