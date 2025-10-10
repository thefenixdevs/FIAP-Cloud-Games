namespace GameStore.Domain.Security;

public interface IPasswordHasher
{
  string Hash(string password);

  bool Verify(string hash, string password);
}
