using GameStore.Application.DTOs;

namespace GameStore.Application.Services;

public interface IGameService
{
    Task<IEnumerable<GameResponse>> GetAllGamesAsync();
    Task<GameResponse?> GetGameByIdAsync(Guid id);
    Task<(bool Success, string Message, GameResponse? Game)> CreateGameAsync(CreateGameRequest request);
    Task<(bool Success, string Message)> UpdateGameAsync(Guid id, UpdateGameRequest request);
    Task<(bool Success, string Message)> DeleteGameAsync(Guid id);
}
