using GameStore.Domain.Services.PasswordService;

namespace GameStore.Tests.TestUtils;

/// <summary>
/// Implementação de teste do IPasswordService para uso em testes unitários.
/// </summary>
public class TestPasswordService : IPasswordService
{
    public string Hash(string password)
    {
        return $"HASH::{password}";
    }

    public bool Verify(string hash, string password)
    {
        return hash == $"HASH::{password}";
    }
}

