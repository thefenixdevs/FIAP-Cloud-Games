using GameStore.Application.Common.Exceptions;
using GameStore.Application.Common.Results;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Auth.UseCases.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApplicationResult<Guid?>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IEncriptService _encriptService;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserCommandHandler> logger,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IEncriptService encriptService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _encriptService = encriptService;
    }

    public async ValueTask<ApplicationResult<Guid?>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Usar TryRegister para acumular violações do domínio
            // Validações de unicidade (EmailAlreadyExists, UsernameAlreadyExists) continuam no FluentValidation
            var (user, validationErrors) = User.TryRegister(
                request.Name,
                request.Email,
                request.Username,
                request.Password,
                _passwordHasher);

            // Se houver violações de validação do domínio, retornar erro
            if (!validationErrors.IsValid)
            {
                _logger.LogWarning("Registration failed: Domain validation errors for email {Email}", request.Email);
                throw new ApplicationValidationException(validationErrors);
            }

            // Verificar se o usuário foi criado (deve ser sempre true neste ponto, mas segurança extra)
            if (user == null)
            {
                _logger.LogError("User creation returned null despite valid validation");
                return ApplicationResult<Guid?>.Failure("AuthService.RegisterAsync.AnErrorOccurredDuringRegistration");
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            var masked = _encriptService.EncodeMaskedCode(request.Email);
            var confirmationLink = $"https://localhost:7055/api/Auth/ValidationAccount?code={masked}";
            var htmlBody = _emailService.TemplateEmailConfirmation(confirmationLink);

            await _emailService.SendConfirmationEmailAsync(
                request.Email,
                "Confirmação de conta",
                htmlBody
            );

            _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
            _logger.LogInformation("User {Username} registered successfully with ID {UserId}", user.Username, user.Id);
            return ApplicationResult<Guid?>.Success(user.Id, "UserRegisteredSuccessfully");
        }
        catch (ApplicationValidationException ex)
        {
            // Violações acumuladas do domínio convertidas para ApplicationResult
            _logger.LogWarning("Registration failed: Validation errors for email {Email}", request.Email);
            return ApplicationResult<Guid?>.ValidationFailure(ex.Errors, "ValidationFailed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            return ApplicationResult<Guid?>.Failure("AuthService.RegisterAsync.AnErrorOccurredDuringRegistration");
        }
    }
}

