using FluentValidation;

namespace GameStore.Application.Features.Users.UseCases.DeleteUser;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Users.Delete.IdIsRequired");
    }
}

