using System;
using System.Net.Mail;

namespace GameStore.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
  public string Value { get; private set; }

  private Email()
  {
    Value = string.Empty;
  }

  private Email(string value)
  {
    Value = value;
  }

  public static Email Create(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
    {
      throw new ArgumentException("Email must be provided.", nameof(email));
    }

    try
    {
      var mailAddress = new MailAddress(email.Trim());
      return new Email(mailAddress.Address.ToLowerInvariant());
    }
    catch (FormatException exception)
    {
      throw new ArgumentException("Email is invalid.", nameof(email), exception);
    }
  }

  public override string ToString() => Value;

  public bool Equals(Email? other)
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

  public override bool Equals(object? obj) => obj is Email other && Equals(other);

  public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

  public static bool operator ==(Email? left, Email? right) => Equals(left, right);

  public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

  public static implicit operator string(Email email) => email.Value;
}
