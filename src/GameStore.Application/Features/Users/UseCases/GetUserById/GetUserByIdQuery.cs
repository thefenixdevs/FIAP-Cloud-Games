using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using Mediator;

namespace GameStore.Application.Features.Users.UseCases.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<ApplicationResult<UserManagementResponse>>;

