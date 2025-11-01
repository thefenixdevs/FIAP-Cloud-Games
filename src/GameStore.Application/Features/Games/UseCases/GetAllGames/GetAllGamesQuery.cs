using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using Mediator;

namespace GameStore.Application.Features.Games.UseCases.GetAllGames;

public sealed record GetAllGamesQuery() : IRequest<ApplicationResult<IEnumerable<GameResponse>>>;

