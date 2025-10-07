using GameStore.Domain.Entities;

namespace GameStore.Domain.Repositories;

public interface IGameRepository
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(Guid id);
    Task AddAsync(Game game);
    Task UpdateAsync(Game game);
    Task DeleteAsync(Game game);
    Task SaveChangesAsync();
}
