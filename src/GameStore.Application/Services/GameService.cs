using GameStore.Application.DTOs;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services;

public class GameService : IGameService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<GameService> _logger;

  public GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IEnumerable<GameResponse>> GetAllGamesAsync()
  {
    _logger.LogInformation("Fetching all games");

    var games = await _unitOfWork.Games.GetAllAsync();
    return games.Select(MapToResponse);
  }

  public async Task<GameResponse?> GetGameByIdAsync(Guid id)
  {
    _logger.LogInformation("Fetching game with ID: {GameId}", id);

    var game = await _unitOfWork.Games.GetByIdAsync(id);
    if (game is null)
    {
      _logger.LogWarning("Game with ID {GameId} not found", id);
      return null;
    }

    return MapToResponse(game);
  }

  public async Task<(bool Success, string Message, GameResponse? Game)> CreateGameAsync(CreateGameRequest request)
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
      return (true, "Game created successfully", MapToResponse(game));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating game: {Title}", request.Title);
      return (false, "An error occurred while creating the game", null);
    }
  }

  public async Task<(bool Success, string Message)> UpdateGameAsync(Guid id, UpdateGameRequest request)
  {
    try
    {
      _logger.LogInformation("Updating game with ID: {GameId}", id);

      var game = await _unitOfWork.Games.GetByIdAsync(id);
      if (game is null)
      {
        _logger.LogWarning("Game with ID {GameId} not found", id);
        return (false, "Game not found");
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

      _logger.LogInformation("Game with ID {GameId} updated successfully", id);
      return (true, "Game updated successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating game with ID: {GameId}", id);
      return (false, "An error occurred while updating the game");
    }
  }

  public async Task<(bool Success, string Message)> DeleteGameAsync(Guid id)
  {
    try
    {
      _logger.LogInformation("Deleting game with ID: {GameId}", id);

      var game = await _unitOfWork.Games.GetByIdAsync(id);
      if (game is null)
      {
        _logger.LogWarning("Game with ID {GameId} not found", id);
        return (false, "Game not found");
      }

      await _unitOfWork.Games.DeleteAsync(game.Id);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("Game with ID {GameId} deleted successfully", id);
      return (true, "Game deleted successfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting game with ID: {GameId}", id);
      return (false, "An error occurred while deleting the game");
    }
  }

  private static GameResponse MapToResponse(Game game)
      => new(
          game.Id,
          game.Title,
          game.Description,
          game.Price,
          game.Genre,
          game.ReleaseDate,
          game.CreatedAt
      );
}
