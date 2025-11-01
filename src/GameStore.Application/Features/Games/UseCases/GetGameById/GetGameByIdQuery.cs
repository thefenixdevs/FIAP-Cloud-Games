using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using Mediator;

namespace GameStore.Application.Features.Games.UseCases.GetGameById;

public sealed record GetGameByIdQuery(Guid Id) : IRequest<ApplicationResult<GameResponse>>;

