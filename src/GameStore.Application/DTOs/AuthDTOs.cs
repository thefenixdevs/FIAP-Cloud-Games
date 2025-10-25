using GameStore.Domain.Enums;

namespace GameStore.Application.DTOs;

public record RegisterRequest(string Name, string Email, string Username, string Password);

public record LoginRequest(string Identifier, string Password);

public record ValidationNotificationRequest(string Email);

public record ValidationAccountRequest(string? Email, DateTime Expiration);

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

public record ValidationAccountResponse(
    Guid Id,
    string Username,
    string Email,
    AccountStatus AccountStatus);

public record ValidationNotificationResponse(
    Guid Id,
    AccountStatus AccountStatus,
    DateTime? ConfirmedAt);
