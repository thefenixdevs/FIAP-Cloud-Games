using FluentValidation;
using GameStore.Application.Features.Users.DTOs;
using GameStore.Domain.SeedWork.Behavior;

namespace GameStore.Application.Features.Users.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly IUnitOfWork _unitOfWork;
    public RegisterRequestValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name)
          .NotEmpty().WithMessage("Nome é obrigatório")
          .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres")
          .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Email)
          .NotEmpty().WithMessage("Email é obrigatório")
          .EmailAddress().WithMessage("Email inválido")
          .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres")
          .MustAsync(IsUniqueEmail).WithMessage("Email já está cadastrado em nossa base!");

        RuleFor(x => x.Username)
          .NotEmpty().WithMessage("Username é obrigatório")
          .MinimumLength(3).WithMessage("Username deve ter no mínimo 3 caracteres")
          .MaximumLength(50).WithMessage("Username deve ter no máximo 50 caracteres")
          .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username deve conter apenas letras, números e underscore")
          .MustAsync(IsUniqueUsername).WithMessage("Username já está cadastrado em nossa base!");

        RuleFor(x => x.Password)
          .NotEmpty().WithMessage("Senha é obrigatória")
          .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
          .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres")
          .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
          .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
          .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
          .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");

        RuleFor(x => x.ConfirmPassword)
          .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
          .Equal(x => x.Password).WithMessage("As senhas não coincidem");
        
    }

    private async Task<bool> IsUniqueEmail(string email, CancellationToken cancellationToken)
    {
      return !await _unitOfWork.Users.ExistsByEmailAsync(email);
    }
    private async Task<bool> IsUniqueUsername(string username, CancellationToken cancellationToken)
    {
      return !await _unitOfWork.Users.ExistsByUsernameAsync(username);
    }
}
