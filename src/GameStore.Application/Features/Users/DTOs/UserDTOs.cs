using GameStore.Domain.Aggregates.UserAggregate.Enums;

namespace GameStore.Application.Features.Users.DTOs;

public record CreateUserRequest(string Name, string Email, string Username, string Password, ProfileType ProfileType);

public record UpdateUserRequest(string Name, string Email, string Username, ProfileType ProfileType, AccountStatus AccountStatus);

public record RegisterRequest(string Name, string Email, string Username, string Password, string ConfirmPassword);

public enum RegistrationStatus
{
    PendingEmailConfirmation,
    Active
}

public sealed record EmailDispatchInfo(
    bool Sent,
    string? FailureReason
);

public sealed record RegisterResult(
    Guid UserId,
    RegistrationStatus Status,
    DateTime? EmailConfirmationExpiresAt,
    EmailDispatchInfo Email
);

public record UserManagementResponse(
    Guid Id,
    string Name,
    string Username,
    string Email,
    ProfileType ProfileType,
    AccountStatus AccountStatus,
    DateTime CreatedAt);
