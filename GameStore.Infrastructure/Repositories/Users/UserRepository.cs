using GameStore.Domain.Entities;
using GameStore.Domain.Repositories;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Repositories.Users;

/// <summary>
/// Implementação do repositório de usuários.
/// Fornece métodos de busca específicos além das operações básicas.
/// </summary>
public class UserRepository : EFRepository<User>, IUserRepository
{
  public UserRepository(GameStoreContext context) : base(context)
  {
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
  }

  public async Task<bool> ExistsByEmailAsync(string email)
  {
    return await _context.Users.AnyAsync(u => u.Email == email);
  }

  public async Task<bool> ExistsByUsernameAsync(string username)
  {
    return await _context.Users.AnyAsync(u => u.Username == username);
  }
}
