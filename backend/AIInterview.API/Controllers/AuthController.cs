using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace AIInterview.API.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")] public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request) { var result = await authService.RegisterAsync(request); return result is null ? Conflict(new { message = "Bu email zaten kayıtlı." }) : Ok(result); }
    [HttpPost("login")] public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) { var result = await authService.LoginAsync(request); return result is null ? Unauthorized(new { message = "Email veya şifre hatalı." }) : Ok(result); }
}
