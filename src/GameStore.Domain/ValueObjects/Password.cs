using System;
using System.Text.RegularExpressions;
using GameStore.Domain.Security;

namespace GameStore.Domain.ValueObjects;

public readonly record struct Password
{
  private const int MinimumPasswordLength = 8;
  private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$";
  private static readonly Regex StrongPasswordRegex = new(PasswordPattern, RegexOptions.Compiled);

  public string Hash { get; }

  private Password(string hash)
  {
    Hash = hash;
  }

  public static Password CreateNew(string password, IPasswordHasher passwordHasher)
  {
    ArgumentNullException.ThrowIfNull(passwordHasher);

    EnsurePasswordStrength(password);
    var hashedPassword = passwordHasher.Hash(password);
    return new Password(hashedPassword);
  }

  public static Password FromHash(string hash)
  {
    if (string.IsNullOrWhiteSpace(hash))
    {
      throw new ArgumentException("Password hash must be provided.", nameof(hash));
    }

    return new Password(hash);
  }

  public bool Verify(string password, IPasswordHasher passwordHasher)
  {
    ArgumentNullException.ThrowIfNull(passwordHasher);
    return passwordHasher.Verify(Hash, password);
  }

  public override string ToString() => Hash;

  private static void EnsurePasswordStrength(string password)
  {
    if (string.IsNullOrWhiteSpace(password))
    {
      throw new ArgumentException("Password must be provided.", nameof(password));
    }

    if (password.Length < MinimumPasswordLength)
    {
      throw new ArgumentException($"Password must be at least {MinimumPasswordLength} characters long.", nameof(password));
    }

    if (!StrongPasswordRegex.IsMatch(password))
    {
      throw new ArgumentException("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.", nameof(password));
    }
  }
}
