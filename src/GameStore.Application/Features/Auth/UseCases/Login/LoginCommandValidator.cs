using FluentValidation;

namespace GameStore.Application.Features.Auth.UseCases.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Auth.Login.IdentifierIsRequired")
            .MaximumLength(320)
            .WithMessage("Auth.Login.IdentifierMaxLengthExceeded");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Auth.Login.PasswordIsRequired")
            .MaximumLength(100)
            .WithMessage("Auth.Login.PasswordMaxLengthExceeded");
    }
}

