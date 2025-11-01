using System.Linq;
using FluentValidation;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Application.Features.Auth.UseCases.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Auth.Register.NameIsRequired")
            .MaximumLength(200)
            .WithMessage("Auth.Register.NameMaxLengthExceeded");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Auth.Register.EmailIsRequired")
            .EmailAddress()
            .WithMessage("Auth.Register.EmailInvalidFormat")
            .MaximumLength(320)
            .WithMessage("Auth.Register.EmailMaxLengthExceeded")
            .MustAsync(async (email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return true;

                var exists = await unitOfWork.Users.ExistsByEmailAsync(email);
                return !exists;
            })
            .WithMessage("EmailAlreadyExists");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Auth.Register.UsernameIsRequired")
            .MinimumLength(3)
            .WithMessage("Auth.Register.UsernameMinLength")
            .MaximumLength(50)
            .WithMessage("Auth.Register.UsernameMaxLengthExceeded")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Auth.Register.UsernameInvalidFormat")
            .MustAsync(async (username, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    return true;

                var exists = await unitOfWork.Users.ExistsByUsernameAsync(username);
                return !exists;
            })
            .WithMessage("UsernameAlreadyExists");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Auth.Register.PasswordIsRequired")
            .MinimumLength(8)
            .WithMessage("Auth.Register.PasswordMustBeAtLeast8CharactersLong")
            .MaximumLength(100)
            .WithMessage("Auth.Register.PasswordMaxLengthExceeded")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsUpper))
            .WithMessage("Auth.Register.PasswordMustContainUpperCase")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsLower))
            .WithMessage("Auth.Register.PasswordMustContainLowerCase")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsDigit))
            .WithMessage("Auth.Register.PasswordMustContainNumber")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(ch => !char.IsLetterOrDigit(ch)))
            .WithMessage("Auth.Register.PasswordMustContainSpecialCharacter");
            

    }
}

