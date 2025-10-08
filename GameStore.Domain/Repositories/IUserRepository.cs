using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Domain.Repositories;

/// <summary>
/// Repository interface for <see cref="User"/> entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByUsernameAsync(string username);
  Task<bool> ExistsByEmailAsync(string email);
  Task<bool> ExistsByUsernameAsync(string username);
}
