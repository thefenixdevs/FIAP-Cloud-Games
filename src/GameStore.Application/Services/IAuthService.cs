using GameStore.Application.DTOs;

namespace GameStore.Application.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, Guid? UserId)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message, ValidationAccountResponse? Response)> ValidationAccountAsync(ValidationAccountRequest request);
    Task<(bool Success, string Message, ValidationNotificationResponse? Response)> SendAccountConfirmationAsync(ValidationNotificationRequest request);
}
