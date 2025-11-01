using GameStore.Domain.Entities;
using GameStore.Domain.Repositories;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Repositories.Games;

/// <summary>
/// Implementação do repositório de jogos.
/// Herda funcionalidades básicas do EFRepository.
/// </summary>
public class GameRepository(GameStoreContext context) : EFRepository<Game>(context), IGameRepository
{
  public async Task<bool> ExistsByTitleAsync(string title)
  {
    var normalizedTitle = title?.Trim().ToLowerInvariant() ?? string.Empty;
    return await _context.Games
      .AnyAsync(g => g.Title.ToLower() == normalizedTitle);
  }

  public async Task<Game?> GetByTitleAsync(string title)
  {
    var normalizedTitle = title?.Trim().ToLowerInvariant() ?? string.Empty;
    return await _context.Games
      .FirstOrDefaultAsync(g => g.Title.ToLower() == normalizedTitle);
  }
}
