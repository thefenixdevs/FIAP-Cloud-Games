using FluentValidation;
using GameStore.Application.Features.Auth.DTOs;

namespace GameStore.Application.Features.Auth.Validators;

public class RequestEmailChangeRequestValidator : AbstractValidator<RequestEmailChangeRequest>
{
  public RequestEmailChangeRequestValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.NewEmail)
      .NotEmpty().WithMessage("Novo e-mail é obrigatório")
      .EmailAddress().WithMessage("Novo e-mail deve ter um formato válido");
  }
}
