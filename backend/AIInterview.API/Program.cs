using AIInterview.API.Data;
using AIInterview.API.Helpers;
using AIInterview.API.Interfaces;
using AIInterview.API.Services;
using Microsoft.EntityFrameworkCore;

DotEnv.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
var configuredGeminiModel = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? builder.Configuration["Gemini:Model"] ?? "gemini-2.5-flash";
app.Logger.LogInformation("[GEMINI MODEL] {Model}", configuredGeminiModel);
Console.WriteLine($"[GEMINI MODEL] {configuredGeminiModel}");
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
// Do not migrate automatically: production deployments should control schema changes.
// The warning makes a missing local migration immediately obvious in the console.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
    try
    {
        var pending = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.GetPendingMigrationsAsync();
        if (pending.Any()) logger.LogWarning("Bekleyen EF Core migration'ları var: {Migrations}. Uygulamak için `dotnet ef database update` çalıştırın.", string.Join(", ", pending));
        else logger.LogInformation("Veritabanı şeması güncel.");
    }
    catch (Exception ex) { logger.LogWarning(ex, "Migration durumu kontrol edilemedi. SQL Server bağlantınızı ve `dotnet ef database update` komutunu kontrol edin."); }
}
app.UseCors("frontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
