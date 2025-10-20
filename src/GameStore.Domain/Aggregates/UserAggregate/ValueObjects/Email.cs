using System;
using System.Net.Mail;
using GameStore.Domain.Exceptions;

namespace GameStore.Domain.Aggregates.UserAggregate.ValueObjects;

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
      throw new DomainRuleException("Email", "Email deve ser fornecido");
    }

    try
    {
      var mailAddress = new MailAddress(email.Trim());
      return new Email(mailAddress.Address.ToLowerInvariant());
    }
    catch (FormatException)
    {
      throw new DomainRuleException("Email", "Email é inválido");
    }
  }

  public override string ToString() => Value;

  public static implicit operator string(Email email) => email.Value;
  public static explicit operator Email(string email) => Create(email);
}
