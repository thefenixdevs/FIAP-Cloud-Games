using GameStore.Application.Common.Results;
using GameStore.Domain.Enums;
using Mediator;

namespace GameStore.Application.Features.Users.UseCases.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    string Username,
    ProfileType ProfileType,
    AccountStatus AccountStatus) : IRequest<ApplicationResult>;

