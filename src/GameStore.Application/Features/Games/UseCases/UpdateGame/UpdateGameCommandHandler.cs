using GameStore.Application.Common.Results;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Games.UseCases.UpdateGame;

public sealed class UpdateGameCommandHandler : IRequestHandler<UpdateGameCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateGameCommandHandler> _logger;

    public UpdateGameCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateGameCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating game with ID: {GameId}", request.Id);

            var game = await _unitOfWork.Games.GetByIdAsync(request.Id);
            if (game is null)
            {
                _logger.LogWarning("Game with ID {GameId} not found", request.Id);
                return ApplicationResult.Failure("GameNotFound");
            }

            game.Update(
                request.Title,
                request.Description ?? string.Empty,
                request.Price,
                request.Genre ?? string.Empty,
                request.ReleaseDate
            );

            await _unitOfWork.Games.UpdateAsync(game);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Game with ID {GameId} updated successfully", request.Id);
            return ApplicationResult.Success("GameUpdatedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game with ID: {GameId}", request.Id);
            return ApplicationResult.Failure("GameService.UpdateGameAsync.AnErrorOccurredWhileUpdatingTheGame");
        }
    }
}

