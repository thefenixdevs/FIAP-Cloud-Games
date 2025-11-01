using FluentValidation;

namespace GameStore.Application.Features.Auth.UseCases.SendAccountConfirmation;

public sealed class SendAccountConfirmationCommandValidator : AbstractValidator<SendAccountConfirmationCommand>
{
    public SendAccountConfirmationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("EmailIsRequired")
            .EmailAddress()
            .WithMessage("EmailInvalidFormat")
            .MaximumLength(320)
            .WithMessage("EmailMaxLengthExceeded");
    }
}

