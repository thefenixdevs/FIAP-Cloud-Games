using System;
using System.Linq;
using System.Text.RegularExpressions;
using GameStore.Domain.Common;
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

    /// <summary>
    /// Tenta criar um Password acumulando violações ao invés de lançar exceções.
    /// Retorna tupla (Password?, ValidationErrors) onde ValidationErrors contém todas as violações encontradas.
    /// </summary>
    public static (Password?, ValidationErrors) TryCreate(string password, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        var errors = ValidationErrors.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            return (null, errors.AddError("Password", "Auth.Register.PasswordIsRequired"));
        }

        if (password.Length > 100)
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMaxLengthExceeded");
        }

        if (password.Length < MinimumPasswordLength)
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMustBeAtLeast8CharactersLong");
        }

        if (!password.Any(char.IsUpper))
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMustContainUpperCase");
        }

        if (!password.Any(char.IsLower))
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMustContainLowerCase");
        }

        if (!password.Any(char.IsDigit))
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMustContainNumber");
        }

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors = errors.AddError("Password", "Auth.Register.PasswordMustContainSpecialCharacter");
        }

        if (!errors.IsValid)
        {
            return (null, errors);
        }

        var hashedPassword = passwordHasher.Hash(password);
        return (new Password(hashedPassword), ValidationErrors.Empty);
    }

    public static Password FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("O hash da senha deve ser fornecido.", nameof(hash));
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
            throw new ArgumentException("A senha deve ser fornecida.", nameof(password));
        }

        if (password.Length < MinimumPasswordLength)
        {
            throw new ArgumentException($"A senha deve ter no m�nimo {MinimumPasswordLength} caracteres.", nameof(password));
        }

        if (!StrongPasswordRegex.IsMatch(password))
        {
            throw new ArgumentException("A senha deve conter pelo menos uma letra mai�scula, uma letra min�scula, um d�gito e um caractere especial.", nameof(password));
        }
    }
}