using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AIInterview.API.Data;
using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using AIInterview.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AIInterview.API.Services;
public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();
    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email)) return null;
        var user = new User { FullName = request.FullName.Trim(), Email = email };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync(); return CreateResponse(user);
    }
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLowerInvariant());
        if (user is null || _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return null;
        return CreateResponse(user);
    }
    private AuthResponse CreateResponse(User user)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT_SECRET is missing.");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? config["Jwt:Issuer"] ?? "AIInterviewSimulator";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? config["Jwt:Audience"] ?? "AIInterviewSimulatorUsers";
        var minutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES") ?? config["Jwt:ExpiresMinutes"], out var value) ? value : 120;
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.FullName), new Claim(JwtRegisteredClaimNames.Email, user.Email) };
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), user.Id, user.FullName, user.Email);
    }
}
