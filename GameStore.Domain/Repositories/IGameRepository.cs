using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Domain.Repositories;

/// <summary>
/// Repository interface for <see cref="Game"/> entities.
/// </summary>
public interface IGameRepository : IRepository<Game>
{
  // Place for future Game-specific queries
}
