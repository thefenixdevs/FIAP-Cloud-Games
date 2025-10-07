using GameStore.Application.DTOs;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GameResponse>> GetAllGamesAsync()
    {
        _logger.LogInformation("Fetching all games");

        var games = await _gameRepository.GetAllAsync();
        return games.Select(g => new GameResponse(
            g.Id,
            g.Title,
            g.Description,
            g.Price,
            g.Genre,
            g.ReleaseDate,
            g.CreatedAt
        ));
    }

    public async Task<GameResponse?> GetGameByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching game with ID: {GameId}", id);

        var game = await _gameRepository.GetByIdAsync(id);

        if (game == null)
        {
            _logger.LogWarning("Game with ID {GameId} not found", id);
            return null;
        }

        return new GameResponse(
            game.Id,
            game.Title,
            game.Description,
            game.Price,
            game.Genre,
            game.ReleaseDate,
            game.CreatedAt
        );
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

            await _gameRepository.AddAsync(game);
            await _gameRepository.SaveChangesAsync();

            _logger.LogInformation("Game created successfully with ID: {GameId}", game.Id);

            var response = new GameResponse(
                game.Id,
                game.Title,
                game.Description,
                game.Price,
                game.Genre,
                game.ReleaseDate,
                game.CreatedAt
            );

            return (true, "Game created successfully", response);
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

            var game = await _gameRepository.GetByIdAsync(id);

            if (game == null)
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

            await _gameRepository.UpdateAsync(game);
            await _gameRepository.SaveChangesAsync();

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

            var game = await _gameRepository.GetByIdAsync(id);

            if (game == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found", id);
                return (false, "Game not found");
            }

            await _gameRepository.DeleteAsync(game);
            await _gameRepository.SaveChangesAsync();

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
