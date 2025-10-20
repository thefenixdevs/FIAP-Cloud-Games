using System;
using System.Text.RegularExpressions;
using GameStore.Domain.Exceptions;
using GameStore.Domain.Services.PasswordService;

namespace GameStore.Domain.Aggregates.UserAggregate.ValueObjects;

public readonly record struct Password
{
  private const int MinimumPasswordLength = 8;
  private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$";
  private static readonly Regex StrongPasswordRegex = new(PasswordPattern, RegexOptions.Compiled);

  private static IPasswordService? _passwordService;

  public string Hash { get; }

  private Password(string hash)
  {
    Hash = hash;
  }

  /// <summary>
  /// Configura o serviço de senha para o value object.
  /// Deve ser chamado na inicialização da aplicação.
  /// </summary>
  public static void ConfigureService(IPasswordService passwordService)
  {
    _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
  }

  public static Password CreateNew(string password)
  {
    if (_passwordService is null)
    {
      throw new InvalidOperationException("PasswordService não foi configurado. Chame Password.ConfigureService na inicialização da aplicação.");
    }

    EnsurePasswordStrength(password);
    var hashedPassword = _passwordService.Hash(password);
    return new Password(hashedPassword);
  }

  public static Password FromHash(string hash)
  {
    if (string.IsNullOrWhiteSpace(hash))
    {
      throw new DomainRuleException("Password", "Hash da senha deve ser fornecido");
    }

    return new Password(hash);
  }

  public bool Verify(string password)
  {
    if (_passwordService is null)
    {
      throw new InvalidOperationException("PasswordService não foi configurado. Chame Password.ConfigureService na inicialização da aplicação.");
    }

    return _passwordService.Verify(Hash, password);
  }

  public override string ToString() => Hash;

  private static void EnsurePasswordStrength(string password)
  {
    if (string.IsNullOrWhiteSpace(password))
    {
      throw new DomainRuleException("Password", "Senha deve ser fornecida");
    }

    if (password.Length < MinimumPasswordLength)
    {
      throw new DomainRuleException("Password", $"Senha deve ter no mínimo {MinimumPasswordLength} caracteres");
    }

    if (!StrongPasswordRegex.IsMatch(password))
    {
      throw new DomainRuleException("Password", "Senha deve conter letras maiúsculas, minúsculas, números e caracteres especiais");
    }
  }
}
