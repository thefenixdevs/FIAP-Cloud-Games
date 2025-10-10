using System;

namespace GameStore.Domain.ValueObjects;

/// <summary>
/// Helper utilities for user name normalization and comparison.
/// </summary>
public static class UsernameNormalizer
{
  public static string Normalize(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
    {
      throw new ArgumentException("Username must be provided.", nameof(username));
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
