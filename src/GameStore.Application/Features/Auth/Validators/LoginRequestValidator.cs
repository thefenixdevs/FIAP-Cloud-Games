using FluentValidation;
using GameStore.Application.Features.Auth.DTOs;

namespace GameStore.Application.Features.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
  public LoginRequestValidator()
  {
    // Configurar modo de cascata para parar na primeira falha (fail-fast)
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Identifier)
      .NotEmpty().WithMessage("Email ou Username é obrigatório")
      .MinimumLength(3).WithMessage("Identificador deve ter no mínimo 3 caracteres");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Senha é obrigatória");
  }
}

