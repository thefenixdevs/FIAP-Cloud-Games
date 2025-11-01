using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using Mediator;

namespace GameStore.Application.Features.Games.UseCases.CreateGame;

public sealed record CreateGameCommand(
    string Title,
    string Description,
    decimal Price,
    string Genre,
    DateTime? ReleaseDate) : IRequest<ApplicationResult<GameResponse>>;

