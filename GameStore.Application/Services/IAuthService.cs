using GameStore.Application.DTOs;

namespace GameStore.Application.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, Guid? UserId)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request);
}
