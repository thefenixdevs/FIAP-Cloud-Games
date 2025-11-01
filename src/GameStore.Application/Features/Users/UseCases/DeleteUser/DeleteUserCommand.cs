using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Users.UseCases.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : IRequest<ApplicationResult>;

