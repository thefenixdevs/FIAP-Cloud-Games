using System;
using System.Threading;
using System.Threading.Tasks;
using GameStore.Domain.Aggregates.GameAggregate.Repositories;
using GameStore.Domain.Aggregates.UserAggregate.Repositories;
using GameStore.Domain.SeedWork.Behavior;
using GameStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameStore.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Unit of Work implementation that coordinates repository operations.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
  private readonly GameStoreContext _context;
  private IDbContextTransaction? _transaction;
  private bool _disposed;

  public UnitOfWork(GameStoreContext context, IUserRepository userRepository, IGameRepository gameRepository)
  {
    _context = context;
    Users = userRepository;
    Games = gameRepository;
  }

  public IUserRepository Users { get; }
  public IGameRepository Games { get; }

  public Task<int> CommitAsync(CancellationToken cancellationToken = default)
      => _context.SaveChangesAsync(cancellationToken);

  public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
  {
    _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
  }

  public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
  {
    if (_transaction == null)
    {
      throw new InvalidOperationException("There is no active transaction to commit.");
    }

    try
    {
      await _context.SaveChangesAsync(cancellationToken);
      await _transaction.CommitAsync(cancellationToken);
    }
    catch
    {
      await RollbackTransactionAsync(cancellationToken);
      throw;
    }
    finally
    {
      await _transaction.DisposeAsync();
      _transaction = null;
    }
  }

  public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
  {
    if (_transaction == null)
    {
      throw new InvalidOperationException("There is no active transaction to roll back.");
    }

    await _transaction.RollbackAsync(cancellationToken);
    await _transaction.DisposeAsync();
    _transaction = null;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      _transaction?.Dispose();
      _context.Dispose();
    }

    _disposed = true;
  }
}
