using GameStore.Domain.Enums;
using GameStore.Domain.ValueObjects;

namespace GameStore.Application.DTOs;

public record CreateUserRequest(string Name, string Email, string Username, string Password, ProfileType ProfileType);

public record UpdateUserRequest(string Name, string Email, string Username, ProfileType ProfileType, AccountStatus AccountStatus);

public record UserManagementResponse(
    Guid Id,
    string Name,
    string Username,
    string Email,
    ProfileType ProfileType,
    AccountStatus AccountStatus,
    DateTime CreatedAt);
