using GameStore.Domain.Security;

namespace GameStore.Tests.TestUtils;

public class TestPasswordHasher : IPasswordHasher
{
  public string Hash(string password)
  {
    return $"HASH::{password}";
  }

  public bool Verify(string hash, string password)
  {
    return hash == Hash(password);
  }
}
