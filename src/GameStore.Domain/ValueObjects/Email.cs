using System;
using System.Net.Mail;

namespace GameStore.Domain.ValueObjects;

public readonly record struct Email
{
  public string Value { get; }

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

  public static implicit operator string(Email email) => email.Value;
  public static explicit operator Email(string email) => Create(email);
}
