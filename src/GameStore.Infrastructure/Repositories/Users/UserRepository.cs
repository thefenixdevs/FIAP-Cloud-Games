using GameStore.Domain.Entities;
using GameStore.Domain.Repositories;
using GameStore.Domain.ValueObjects;
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
    // Normalizar email (mesma lógica do Email.Create mas sem validação)
    var (emailVo, emailErrors) = Email.TryCreate(email);
    if (!emailErrors.IsValid)
    {
      return null;
    }
    
    // Email é armazenado como string normalizada no banco (através do HasConversion)
    // Como Email.Create já normaliza para lowercase, comparar diretamente usando EF.Functions
    return await _context.Users
      .FirstOrDefaultAsync(u => u.Email == emailVo);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    var normalizedUsername = UsernameNormalizer.NormalizeForComparison(username);
    return await _context.Users.FirstOrDefaultAsync(
      u => u.Username == normalizedUsername);
  }

  public async Task<bool> ExistsByEmailAsync(string email)
  {
    // Normalizar email (mesma lógica do Email.Create mas sem validação)
    var (emailVo, emailErrors) = Email.TryCreate(email);
    if (!emailErrors.IsValid)
    {
      return false;
    }
    // Usar LINQ diretamente já que Email é armazenado como string no banco
    return await _context.Users
      .AnyAsync(u => u.Email == emailVo);
  }

  public async Task<bool> ExistsByUsernameAsync(string username)
  {
    var normalizedUsername = UsernameNormalizer.NormalizeForComparison(username);
    return await _context.Users.AnyAsync(
      u => u.Username == normalizedUsername);
  }
}
