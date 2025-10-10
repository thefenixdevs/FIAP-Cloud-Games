using System;
using System.Text.RegularExpressions;
using GameStore.Domain.Security;

namespace GameStore.Domain.ValueObjects;

public sealed class Password : IEquatable<Password>
{
  private const int MinimumPasswordLength = 8;
  private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$";
  private static readonly Regex StrongPasswordRegex = new(PasswordPattern, RegexOptions.Compiled);

  public string Hash { get; private set; }

  private Password()
  {
    Hash = string.Empty;
  }

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

  public bool Equals(Password? other)
  {
    if (ReferenceEquals(null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return string.Equals(Hash, other.Hash, StringComparison.Ordinal);
  }

  public override bool Equals(object? obj) => obj is Password other && Equals(other);

  public override int GetHashCode() => Hash.GetHashCode(StringComparison.Ordinal);

  public static bool operator ==(Password? left, Password? right) => Equals(left, right);

  public static bool operator !=(Password? left, Password? right) => !Equals(left, right);

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
