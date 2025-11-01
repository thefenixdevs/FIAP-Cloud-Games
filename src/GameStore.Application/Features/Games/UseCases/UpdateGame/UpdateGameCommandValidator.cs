using FluentValidation;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Application.Features.Games.UseCases.UpdateGame;

public sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Games.UpdateGame.IdIsRequired");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Games.CreateUpdateGame.TitleIsRequired")
            .MaximumLength(200)
            .WithMessage("Games.CreateUpdateGame.TitleMaxLengthExceeded")
            .MustAsync(async (command, title, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(title))
                    return true;

                // Buscar o jogo atual
                var game = await unitOfWork.Games.GetByIdAsync(command.Id);
                if (game == null)
                    return true; // Validação de existência é feita no handler

                // Só validar unicidade se o título mudou
                if (game.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
                    return true;

                var exists = await unitOfWork.Games.ExistsByTitleAsync(title);
                return !exists;
            })
            .WithMessage("Games.CreateUpdateGame.TitleAlreadyExists");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Games.CreateUpdateGame.DescriptionMaxLengthExceeded")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Game.CreateUpdateGame.PriceCannotBeNegative")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Game.CreateUpdateGame.PriceMaxValueExceeded");

        RuleFor(x => x.Genre)
            .MaximumLength(100)
            .WithMessage("Games.CreateUpdateGame.GenreMaxLengthExceeded")
            .When(x => !string.IsNullOrWhiteSpace(x.Genre));

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(10))
            .WithMessage("Games.CreateUpdateGame.ReleaseDateInvalid")
            .When(x => x.ReleaseDate.HasValue);
    }
}

