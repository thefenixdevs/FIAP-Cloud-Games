using GameStore.Domain.Services.PasswordService;

namespace GameStore.Infrastructure.Services.PasswordServices;

/// <summary>
/// Implementação do serviço de senha usando BCrypt.
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public string Hash(string password)
    {
        return _passwordHasher.Hash(password);
    }

    public bool Verify(string hash, string password)
    {
        return _passwordHasher.Verify(hash, password);
    }
}

