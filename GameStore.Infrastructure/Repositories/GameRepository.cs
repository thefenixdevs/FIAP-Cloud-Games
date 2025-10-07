using GameStore.Domain.Entities;
using GameStore.Domain.Repositories;
using GameStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GameStoreContext _context;

    public GameRepository(GameStoreContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task AddAsync(Game game)
    {
        await _context.Games.AddAsync(game);
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
    }

    public async Task DeleteAsync(Game game)
    {
        _context.Games.Remove(game);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
