using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using GameStore.Domain.Enums;
using Mediator;

namespace GameStore.Application.Features.Users.UseCases.CreateUser;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string Username,
    string Password,
    ProfileType ProfileType) : IRequest<ApplicationResult<UserManagementResponse>>;

