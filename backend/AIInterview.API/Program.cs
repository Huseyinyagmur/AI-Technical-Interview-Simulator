using AIInterview.API.Data;
using AIInterview.API.Helpers;
using AIInterview.API.Interfaces;
using AIInterview.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

DotEnv.Load();
QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => { options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header }); options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } }); });
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Secret"] ?? "";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "AIInterviewSimulator";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "AIInterviewSimulatorUsers";
if (!string.IsNullOrWhiteSpace(jwtSecret)) builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = jwtIssuer, ValidateAudience = true, ValidAudience = jwtAudience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)), ValidateLifetime = true });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IInterviewService, InterviewService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
