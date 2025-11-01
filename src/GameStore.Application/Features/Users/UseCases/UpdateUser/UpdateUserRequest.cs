using GameStore.Domain.Enums;

namespace GameStore.Application.Features.Users.UseCases.UpdateUser;

public record UpdateUserRequest(string Name, string Email, string Username, ProfileType ProfileType, AccountStatus AccountStatus);

