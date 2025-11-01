using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Auth.UseCases.RegisterUser;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Username,
    string Password) : IRequest<ApplicationResult<Guid?>>;

