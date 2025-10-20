using FluentValidation;
using GameStore.Application.Features.Users.DTOs;

namespace GameStore.Application.Features.Users.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
  public CreateUserRequestValidator()
  {
    // Configurar modo de cascata para parar na primeira falha (fail-fast)
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Nome é obrigatório")
      .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres")
      .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email é obrigatório")
      .EmailAddress().WithMessage("Email inválido")
      .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres");

    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("Username é obrigatório")
      .MinimumLength(3).WithMessage("Username deve ter no mínimo 3 caracteres")
      .MaximumLength(50).WithMessage("Username deve ter no máximo 50 caracteres")
      .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username deve conter apenas letras, números e underscore");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Senha é obrigatória")
      .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
      .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres")
      .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
      .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
      .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
      .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");

    RuleFor(x => x.ProfileType)
      .IsInEnum().WithMessage("Tipo de perfil inválido");
  }
}

