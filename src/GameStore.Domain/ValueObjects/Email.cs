using System.Net.Mail;
using GameStore.Domain.Common;

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

  /// <summary>
  /// Tenta criar um Email acumulando violações ao invés de lançar exceções.
  /// Retorna tupla (Email?, ValidationErrors) onde ValidationErrors contém todas as violações encontradas.
  /// </summary>
  public static (Email?, ValidationErrors) TryCreate(string email)
  {
    var errors = ValidationErrors.Empty;

    if (string.IsNullOrWhiteSpace(email))
    {
      return (null, errors.AddError("Email", "Auth.Register.EmailIsRequired"));
    }

    var trimmedEmail = email.Trim();

    if (trimmedEmail.Length > 320)
    {
      errors = errors.AddError("Email", "Auth.Register.EmailMaxLengthExceeded");
    }

    try
    {
      var mailAddress = new MailAddress(trimmedEmail);
      var emailValue = new Email(mailAddress.Address.ToLowerInvariant());
      return (emailValue, errors);
    }
    catch (FormatException)
    {
      errors = errors.AddError("Email", "Auth.Register.EmailInvalidFormat");
      return (null, errors);
    }
  }

  public override string ToString() => Value;

  public static implicit operator string(Email email) => email.Value;
  public static explicit operator Email(string email) => Create(email);
}
