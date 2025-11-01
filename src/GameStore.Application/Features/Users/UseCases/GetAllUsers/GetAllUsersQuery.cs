using GameStore.Application.Common.Results;
using GameStore.Application.Features.Users.Shared;
using Mediator;

namespace GameStore.Application.Features.Users.UseCases.GetAllUsers;

public sealed record GetAllUsersQuery() : IRequest<ApplicationResult<IEnumerable<UserManagementResponse>>>;

