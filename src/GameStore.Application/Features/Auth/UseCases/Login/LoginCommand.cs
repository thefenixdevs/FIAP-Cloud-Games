using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Auth.UseCases.Login;

public sealed record LoginCommand(
    string Identifier,
    string Password) : IRequest<ApplicationResult<LoginResponse>>;

