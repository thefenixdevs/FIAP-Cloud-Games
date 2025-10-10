using System;

namespace GameStore.Domain.ValueObjects;

public sealed class Username : IEquatable<Username>
{
  public string Value { get; private set; }

  private Username()
  {
    Value = string.Empty;
  }

  private Username(string value)
  {
    Value = value;
  }

  public static Username Create(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      throw new ArgumentException("Username must be provided.", nameof(username));
    }

    return new Username(username.Trim().ToLower());
  }

  public override string ToString() => Value;

  public bool Equals(Username? other)
  {
    if (ReferenceEquals(null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
  }

  public override bool Equals(object? obj) => obj is Username other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

  public static bool operator ==(Username? left, Username? right) => Equals(left, right);

  public static bool operator !=(Username? left, Username? right) => !Equals(left, right);

  public static implicit operator string(Username username) => username.Value;
}
