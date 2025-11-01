using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Games.UseCases.DeleteGame;

public sealed record DeleteGameCommand(Guid Id) : IRequest<ApplicationResult>;

