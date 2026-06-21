using AIInterview.API.DTOs;
namespace AIInterview.API.Interfaces;
public interface IAuthService { Task<AuthResponse?> RegisterAsync(RegisterRequest request); Task<AuthResponse?> LoginAsync(LoginRequest request); }
