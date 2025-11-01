using FluentValidation;

namespace GameStore.Application.Features.Games.UseCases.DeleteGame;

public sealed class DeleteGameCommandValidator : AbstractValidator<DeleteGameCommand>
{
    public DeleteGameCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Games.DeleteGame.IdIsRequired");
    }
}

