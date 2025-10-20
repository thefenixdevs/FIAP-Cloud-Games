using GameStore.Domain.Aggregates.GameAggregate;
using GameStore.Domain.SeedWork.Repositories;

namespace GameStore.Domain.Aggregates.GameAggregate.Repositories;

/// <summary>
/// Repository interface for <see cref="Game"/> entities.
/// </summary>
public interface IGameRepository : IRepository<Game>
{
  // Place for future Game-specific queries
}
