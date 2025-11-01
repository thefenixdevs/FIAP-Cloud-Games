using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Domain.Repositories;

/// <summary>
/// Repository interface for <see cref="Game"/> entities.
/// </summary>
public interface IGameRepository : IRepository<Game>
{
  /// <summary>
  /// Verifica se existe um jogo com o título especificado (case-insensitive).
  /// </summary>
  /// <param name="title">Título do jogo a ser verificado</param>
  Task<bool> ExistsByTitleAsync(string title);

  /// <summary>
  /// Obtém um jogo pelo título especificado (case-insensitive).
  /// </summary>
  /// <param name="title">Título do jogo</param>
  Task<Game?> GetByTitleAsync(string title);
}
