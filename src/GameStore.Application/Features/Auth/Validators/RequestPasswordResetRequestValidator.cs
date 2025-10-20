using FluentValidation;
using GameStore.Application.Features.Auth.DTOs;

namespace GameStore.Application.Features.Auth.Validators;

public class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
  public RequestPasswordResetRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("E-mail é obrigatório")
      .EmailAddress().WithMessage("E-mail deve ter um formato válido");
  }
}
