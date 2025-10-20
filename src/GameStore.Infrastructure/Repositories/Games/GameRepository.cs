using GameStore.Domain.Aggregates.GameAggregate;
using GameStore.Domain.Aggregates.GameAggregate.Repositories;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Repositories.Abstractions;

namespace GameStore.Infrastructure.Repositories.Games;

/// <summary>
/// Implementação do repositório de jogos.
/// Herda funcionalidades básicas do EFRepository.
/// </summary>
public class GameRepository(GameStoreContext context) : EFRepository<Game>(context), IGameRepository
{
  // Implementações específicas para Game podem ser adicionadas aqui
  // Exemplo:
  // public async Task<IEnumerable<Game>> GetByGenreAsync(string genre)
  // {
  //     return await _context.Games
  //         .Where(g => g.Genre == genre)
  //         .ToListAsync();
  // }
}
