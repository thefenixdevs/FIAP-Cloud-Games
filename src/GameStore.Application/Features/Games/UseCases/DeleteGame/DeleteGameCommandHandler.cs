using GameStore.Application.Common.Results;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Games.UseCases.DeleteGame;

public sealed class DeleteGameCommandHandler : IRequestHandler<DeleteGameCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGameCommandHandler> _logger;

    public DeleteGameCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteGameCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<ApplicationResult> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting game with ID: {GameId}", request.Id);

            var game = await _unitOfWork.Games.GetByIdAsync(request.Id);
            if (game is null)
            {
                _logger.LogWarning("Game with ID {GameId} not found", request.Id);
                return ApplicationResult.Failure("GameNotFound");
            }

            await _unitOfWork.Games.DeleteAsync(game.Id);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Game with ID {GameId} deleted successfully", request.Id);
            return ApplicationResult.Success("GameService.DeleteGameAsync.GameDeletedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game with ID: {GameId}", request.Id);
            return ApplicationResult.Failure("GameService.DeleteGameAsync.AnErrorOccurredWhileDeletingTheGame");
        }
    }
}

