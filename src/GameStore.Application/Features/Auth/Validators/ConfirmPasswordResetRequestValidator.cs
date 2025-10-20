using FluentValidation;
using GameStore.Application.Features.Auth.DTOs;

namespace GameStore.Application.Features.Auth.Validators;

public class ConfirmPasswordResetRequestValidator : AbstractValidator<ConfirmPasswordResetRequest>
{
  public ConfirmPasswordResetRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Token)
      .NotEmpty().WithMessage("Token é obrigatório");

    RuleFor(x => x.NewPassword)
      .NotEmpty().WithMessage("Nova senha é obrigatória")
      .MinimumLength(8).WithMessage("Nova senha deve ter no mínimo 8 caracteres")
      .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
      .WithMessage("Nova senha deve conter pelo menos: 1 letra minúscula, 1 maiúscula, 1 número e 1 caractere especial");

    RuleFor(x => x.ConfirmPassword)
      .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
      .Equal(x => x.NewPassword).WithMessage("As senhas não coincidem");
  }
}
