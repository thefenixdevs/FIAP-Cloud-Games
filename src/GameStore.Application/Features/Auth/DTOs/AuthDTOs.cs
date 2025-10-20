using GameStore.Domain.Aggregates.UserAggregate.Enums;

namespace GameStore.Application.Features.Auth.DTOs;

public record LoginRequest(string Identifier, string Password);

public record LoginResponse(
    Guid UserId,
    string Username,
    string Email,
    string Token,
    ProfileType ProfileType,
    AccountStatus AccountStatus);

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    ProfileType ProfileType,
    AccountStatus AccountStatus,
    DateTime CreatedAt);

public record RequestPasswordResetRequest(string Email);

public record ConfirmPasswordResetRequest(string Token, string NewPassword, string ConfirmPassword);

public record RequestEmailChangeRequest(string NewEmail);

public record ConfirmEmailChangeRequest(string Token);

