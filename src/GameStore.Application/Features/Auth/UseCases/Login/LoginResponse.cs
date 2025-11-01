using GameStore.Domain.Enums;

namespace GameStore.Application.Features.Auth.UseCases.Login;

public record LoginResponse(
    Guid UserId,
    string Username,
    string Email,
    string Token,
    ProfileType ProfileType,
    AccountStatus AccountStatus);

