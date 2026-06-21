using AIInterview.API.DTOs;
using AIInterview.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace AIInterview.API.Controllers;
[ApiController, Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")] public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request) { try { var result = await authService.RegisterAsync(request); return result is null ? Conflict(new { message = "Bu email zaten kayıtlı." }) : Ok(result); } catch (InvalidOperationException ex) { return Problem(title: "Kimlik doğrulama yapılandırması eksik", detail: ex.Message, statusCode: 500); } }
    [HttpPost("login")] public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) { try { var result = await authService.LoginAsync(request); return result is null ? Unauthorized(new { message = "Email veya şifre hatalı." }) : Ok(result); } catch (InvalidOperationException ex) { return Problem(title: "Kimlik doğrulama yapılandırması eksik", detail: ex.Message, statusCode: 500); } }
}
