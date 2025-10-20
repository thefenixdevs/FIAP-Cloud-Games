using FluentValidation;
using GameStore.Application.Features.Games.DTOs;

namespace GameStore.Application.Features.Games.Validators;

public class UpdateGameRequestValidator : AbstractValidator<UpdateGameRequest>
{
  public UpdateGameRequestValidator()
  {
    // Configurar modo de cascata para parar na primeira falha (fail-fast)
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Title)
      .NotEmpty().WithMessage("Título é obrigatório")
      .MinimumLength(1).WithMessage("Título deve ter no mínimo 1 caractere")
      .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres");

    RuleFor(x => x.Description)
      .MaximumLength(1000).WithMessage("Descrição deve ter no máximo 1000 caracteres");

    RuleFor(x => x.Price)
      .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo");

    RuleFor(x => x.Genre)
      .MaximumLength(100).WithMessage("Gênero deve ter no máximo 100 caracteres");

    RuleFor(x => x.ReleaseDate)
      .LessThanOrEqualTo(DateTime.UtcNow.AddYears(10)).WithMessage("Data de lançamento não pode ser maior que 10 anos no futuro")
      .When(x => x.ReleaseDate.HasValue);
  }
}

