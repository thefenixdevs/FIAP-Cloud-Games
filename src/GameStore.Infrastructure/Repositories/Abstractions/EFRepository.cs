using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Generic repository implementation backed by Entity Framework Core.
/// </summary>
public class EFRepository<T> : IRepository<T> where T : BaseEntity
{
  protected readonly GameStoreContext _context;
  protected readonly DbSet<T> _dbSet;

  protected EFRepository(GameStoreContext context)
  {
    _context = context;
    _dbSet = _context.Set<T>();
  }

  public async Task AddAsync(T entity)
  {
    entity.CreatedAt = DateTime.UtcNow;
    await _dbSet.AddAsync(entity);
  }

  public async Task DeleteAsync(Guid id)
  {
    var entity = await _dbSet.FindAsync(id);

    if (entity == null)
    {
      return;
    }

    _dbSet.Remove(entity);
  }

  public async Task<ICollection<T>> GetAllAsync()
      => await _dbSet.AsNoTracking().ToListAsync();

  public async Task<T?> GetByIdAsync(Guid id)
      => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

  public Task UpdateAsync(T entity)
  {
    entity.UpdatedAt = DateTime.UtcNow;
    _dbSet.Update(entity);
    return Task.CompletedTask;
  }
}
