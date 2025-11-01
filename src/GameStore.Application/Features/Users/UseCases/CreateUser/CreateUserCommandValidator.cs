using System.Linq;
using FluentValidation;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Application.Features.Users.UseCases.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Users.CreateUpdateUser.NameIsRequired")
            .MaximumLength(200)
            .WithMessage("Users.CreateUpdateUser.NameMaxLengthExceeded");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("EmailIsRequired")
            .EmailAddress()
            .WithMessage("EmailInvalidFormat")
            .MaximumLength(320)
            .WithMessage("EmailMaxLengthExceeded")
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
            .WithMessage("Users.CreateUpdateUser.UsernameIsRequired")
            .MinimumLength(3)
            .WithMessage("Users.CreateUpdateUser.UsernameMinLength")
            .MaximumLength(50)
            .WithMessage("Users.CreateUpdateUser.UsernameMaxLengthExceeded")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Users.CreateUpdateUser.UsernameInvalidFormat")
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
            .WithMessage("Users.CreateUpdateUser.PasswordIsRequired")
            .MinimumLength(8)
            .WithMessage("Users.CreateUpdateUser.PasswordMustBeAtLeast8CharactersLong")
            .MaximumLength(100)
            .WithMessage("Users.CreateUpdateUser.PasswordMaxLengthExceeded")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsUpper))
            .WithMessage("Users.CreateUpdateUser.PasswordMustContainUpperCase")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsLower))
            .WithMessage("Users.CreateUpdateUser.PasswordMustContainLowerCase")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(char.IsDigit))
            .WithMessage("Users.CreateUpdateUser.PasswordMustContainNumber")
            .Must(password => !string.IsNullOrWhiteSpace(password) && password.Any(ch => !char.IsLetterOrDigit(ch)))
            .WithMessage("Users.CreateUpdateUser.PasswordMustContainSpecialCharacter");

    }
}

