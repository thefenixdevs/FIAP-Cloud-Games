using System;
using GameStore.Domain.Exceptions;

namespace GameStore.Domain.Aggregates.UserAggregate.ValueObjects;

/// <summary>
/// Helper utilities for user name normalization and comparison.
/// </summary>
public static class UsernameNormalizer
{
  public static string Normalize(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      throw new DomainRuleException("Username", "Nome de usuário deve ser fornecido");
    }

    return username.Trim();
  }

  public static string NormalizeForComparison(string username)
  {
    return Normalize(username).ToLowerInvariant();
  }

  public static bool AreEqual(string left, string right)
  {
    return string.Equals(NormalizeForComparison(left), NormalizeForComparison(right), StringComparison.Ordinal);
  }
}
