using FluentValidation;

namespace GameStore.Application.Features.Auth.UseCases.ValidationAccount;

public sealed class ValidationAccountCommandValidator : AbstractValidator<ValidationAccountCommand>
{
    public ValidationAccountCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("CodeIsRequired")
            .MinimumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("InvalidCodeFormat");
    }
}

