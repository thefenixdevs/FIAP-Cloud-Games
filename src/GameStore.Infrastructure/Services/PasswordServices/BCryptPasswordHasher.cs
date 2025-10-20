using GameStore.Domain.Services.PasswordService;

namespace GameStore.Infrastructure.Services.PasswordServices;

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
