using GameStore.Domain.Enums;

namespace GameStore.Application.Features.Users.Shared;

public record UserManagementResponse(
    Guid Id,
    string Name,
    string Username,
    string Email,
    ProfileType ProfileType,
    AccountStatus AccountStatus,
    DateTime CreatedAt);

