using GameStore.Application.Features.Auth.DTOs;

namespace GameStore.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ConfirmEmailAsync(Guid userId, string token);
    Task RequestPasswordResetAsync(RequestPasswordResetRequest request);
    Task ConfirmPasswordResetAsync(Guid userId, ConfirmPasswordResetRequest request);
    Task RequestEmailChangeAsync(Guid userId, RequestEmailChangeRequest request);
    Task ConfirmEmailChangeAsync(Guid userId, string token);
}
