using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Games.UseCases.CreateGame;

public sealed class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, ApplicationResult<GameResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateGameCommandHandler> _logger;

    public CreateGameCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateGameCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult<GameResponse>> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new game: {Title}", request.Title);

            var game = new Game(
                request.Title,
                request.Description ?? string.Empty,
                request.Price,
                request.Genre ?? string.Empty,
                request.ReleaseDate
            );

            await _unitOfWork.Games.AddAsync(game);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Game created successfully with ID: {GameId}", game.Id);
            
            var response = game.Adapt<GameResponse>();

            return ApplicationResult<GameResponse>.Success(response, "GameService.CreateGameAsync.GameCreatedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game: {Title}", request.Title);
            return ApplicationResult<GameResponse>.Failure("GameService.CreateGameAsync.AnErrorOccurredWhileCreatingTheGame");
        }
    }
}

