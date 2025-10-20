namespace GameStore.Domain.Services.PasswordService;

/// <summary>
/// Serviço de domínio responsável por operações com senha.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Gera o hash de uma senha.
    /// </summary>
    /// <param name="password">A senha em texto plano.</param>
    /// <returns>O hash da senha.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifica se uma senha corresponde ao hash armazenado.
    /// </summary>
    /// <param name="hash">O hash armazenado.</param>
    /// <param name="password">A senha em texto plano para verificar.</param>
    /// <returns>True se a senha corresponde ao hash, caso contrário false.</returns>
    bool Verify(string hash, string password);
}

