using GameStore.Domain.SeedWork.Entities;

namespace GameStore.Domain.SeedWork.Repositories;

/// <summary>
/// Interface base para repositórios genéricos.
/// Define operações CRUD padrão para todas as entidades.
/// </summary>
/// <typeparam name="T">Tipo da entidade que herda de BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
  /// <summary>
  /// Obtém todas as entidades
  /// </summary>
  Task<ICollection<T>> GetAllAsync();

  /// <summary>
  /// Obtém uma entidade por ID
  /// </summary>
  /// <param name="id">ID da entidade</param>
  Task<T?> GetByIdAsync(Guid id);

  /// <summary>
  /// Adiciona uma nova entidade
  /// </summary>
  /// <param name="entity">Entidade a ser adicionada</param>
  Task AddAsync(T entity);

  /// <summary>
  /// Atualiza uma entidade existente
  /// </summary>
  /// <param name="entity">Entidade a ser atualizada</param>
  Task UpdateAsync(T entity);

  /// <summary>
  /// Remove uma entidade por ID
  /// </summary>
  /// <param name="id">ID da entidade a ser removida</param>
  Task DeleteAsync(Guid id);
}
