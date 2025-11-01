using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Games.UseCases.UpdateGame;

public sealed record UpdateGameCommand(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Genre,
    DateTime? ReleaseDate) : IRequest<ApplicationResult>;

