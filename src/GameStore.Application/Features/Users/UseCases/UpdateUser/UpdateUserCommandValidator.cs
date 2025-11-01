using FluentValidation;
using GameStore.Domain.Repositories.Abstractions;

namespace GameStore.Application.Features.Users.UseCases.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Users.Update.IdIsRequired");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Users.CreateUpdateUser.NameIsRequired")
            .MaximumLength(200)
            .WithMessage("Users.CreateUpdateUser.NameMaxLengthExceeded");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("EmailIsRequired")
            .EmailAddress()
            .WithMessage("EmailInvalidFormat")
            .MaximumLength(320)
            .WithMessage("EmailMaxLengthExceeded")
            .MustAsync(async (command, email, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(email))
                    return true;

                // Buscar o usuário atual
                var user = await unitOfWork.Users.GetByIdAsync(command.Id);
                if (user == null)
                    return true; // Validação de existência é feita no handler

                // Só validar unicidade se o email mudou
                if (user.Email.Value == email)
                    return true;

                var exists = await unitOfWork.Users.ExistsByEmailAsync(email);
                return !exists;
            })
            .WithMessage("EmailAlreadyExists");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Users.CreateUpdateUser.UsernameIsRequired")
            .MinimumLength(3)
            .WithMessage("Users.CreateUpdateUser.UsernameMinLength")
            .MaximumLength(50)
            .WithMessage("Users.CreateUpdateUser.UsernameMaxLengthExceeded")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Users.CreateUpdateUser.UsernameInvalidFormat")
            .MustAsync(async (command, username, cancellation) =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    return true;

                // Buscar o usuário atual
                var user = await unitOfWork.Users.GetByIdAsync(command.Id);
                if (user == null)
                    return true; // Validação de existência é feita no handler

                // Só validar unicidade se o username mudou
                if (user.Username == username)
                    return true;

                var exists = await unitOfWork.Users.ExistsByUsernameAsync(username);
                return !exists;
            })
            .WithMessage("UsernameAlreadyExists");
    }
}

