using System;
using System.Threading;
using System.Threading.Tasks;
using GameStore.Domain.Repositories;

namespace GameStore.Domain.Repositories.Abstractions;

/// <summary>
/// Coordinates work across repositories and ensures transactional consistency.
/// </summary>
public interface IUnitOfWork : IDisposable
{
  IUserRepository Users { get; }
  IGameRepository Games { get; }
  Task<int> CommitAsync(CancellationToken cancellationToken = default);
  Task BeginTransactionAsync(CancellationToken cancellationToken = default);
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);
  Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
