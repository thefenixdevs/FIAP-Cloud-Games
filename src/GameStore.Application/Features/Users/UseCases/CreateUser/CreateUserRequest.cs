using GameStore.Domain.Enums;

namespace GameStore.Application.Features.Users.UseCases.CreateUser;

public record CreateUserRequest(string Name, string Email, string Username, string Password, ProfileType ProfileType);

