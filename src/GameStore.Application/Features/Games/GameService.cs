using FluentValidation;
using GameStore.Application.Features.Games.DTOs;
using GameStore.Application.Features.Games.Interfaces;
using Microsoft.Extensions.Logging;
using Mapster;
using GameStore.Domain.Aggregates.GameAggregate;
using GameStore.Domain.SeedWork.Behavior;

namespace GameStore.Application.Features.Games;

public class GameService : IGameService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<GameService> _logger;
  private readonly TypeAdapterConfig _mapperConfig;
  private readonly IValidator<CreateGameRequest> _createGameValidator;
  private readonly IValidator<UpdateGameRequest> _updateGameValidator;

  public GameService(
    IUnitOfWork unitOfWork, 
    ILogger<GameService> logger, 
    TypeAdapterConfig mapperConfig,
    IValidator<CreateGameRequest> createGameValidator,
    IValidator<UpdateGameRequest> updateGameValidator)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
    _mapperConfig = mapperConfig;
    _createGameValidator = createGameValidator;
    _updateGameValidator = updateGameValidator;
  }

  public async Task<IEnumerable<GameResponse>> GetAllGamesAsync()
  {
    _logger.LogInformation("Fetching all games");

    var games = await _unitOfWork.Games.GetAllAsync();
    return games.Select(game => game.Adapt<GameResponse>(_mapperConfig));
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

    return game.Adapt<GameResponse>(_mapperConfig);
  }

  public async Task<(bool Success, string Message, GameResponse? Game)> CreateGameAsync(CreateGameRequest request)
  {
    try
    {
      // Validação automática - lança ValidationException se inválido
      await _createGameValidator.ValidateAndThrowAsync(request);

      _logger.LogInformation("Creating new game: {Title}", request.Title);

      var game = request.Adapt<Game>(_mapperConfig);

      await _unitOfWork.Games.AddAsync(game);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("Game created successfully with ID: {GameId}", game.Id);
      return (true, "Game created successfully", game.Adapt<GameResponse>(_mapperConfig));
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
      // Validação automática - lança ValidationException se inválido
      await _updateGameValidator.ValidateAndThrowAsync(request);

      _logger.LogInformation("Updating game with ID: {GameId}", id);

      var game = await _unitOfWork.Games.GetByIdAsync(id);
      if (game is null)
      {
        _logger.LogWarning("Game with ID {GameId} not found", id);
        return (false, "Game not found");
      }

      request.Adapt(game, _mapperConfig);

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

}
