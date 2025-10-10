using GameStore.Domain.Security;

namespace GameStore.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
  public string Hash(string password)
  {
    return BCrypt.Net.BCrypt.HashPassword(password);
  }

  public bool Verify(string hash, string password)
  {
    return BCrypt.Net.BCrypt.Verify(password, hash);
  }
}
