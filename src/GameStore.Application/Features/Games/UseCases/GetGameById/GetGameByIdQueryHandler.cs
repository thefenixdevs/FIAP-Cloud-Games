using GameStore.Application.Common.Results;
using GameStore.Application.Features.Games.Shared;
using GameStore.Domain.Repositories.Abstractions;
using Mapster;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Games.UseCases.GetGameById;

public sealed class GetGameByIdQueryHandler : IRequestHandler<GetGameByIdQuery, ApplicationResult<GameResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetGameByIdQueryHandler> _logger;

    public GetGameByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetGameByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult<GameResponse>> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching game with ID: {GameId}", request.Id);

            var game = await _unitOfWork.Games.GetByIdAsync(request.Id);
            if (game is null)
            {
                _logger.LogWarning("Game with ID {GameId} not found", request.Id);
                return ApplicationResult<GameResponse>.Failure("GameNotFound");
            }

            var response = game.Adapt<GameResponse>();
            return ApplicationResult<GameResponse>.Success(response, "Game retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching game with ID: {GameId}", request.Id);
            return ApplicationResult<GameResponse>.Failure("An error occurred while fetching the game");
        }
    }
}

