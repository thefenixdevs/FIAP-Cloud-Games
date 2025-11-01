using GameStore.Application.Common.Results;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Auth.UseCases.ValidationAccount;

public sealed class ValidationAccountCommandHandler : IRequestHandler<ValidationAccountCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ValidationAccountCommandHandler> _logger;
    private readonly IEncriptService _encriptService;

    public ValidationAccountCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ValidationAccountCommandHandler> logger,
        IEncriptService encriptService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _encriptService = encriptService;
    }

    public async ValueTask<ApplicationResult> Handle(ValidationAccountCommand request, CancellationToken cancellationToken)
    {
        User? user = null;

        try
        {
            // Decodificar o código recebido
            if (string.IsNullOrWhiteSpace(request.Code))
            {
                _logger.LogWarning("Validation attempt with empty code");
                return ApplicationResult.Failure("AuthService.ValidationAccount.InvalidCode");
            }

            var decoded = _encriptService.DecodeMaskedCode(request.Code);
            if (decoded == null)
            {
                _logger.LogWarning("Invalid code format received: {Code}", request.Code);
                return ApplicationResult.Failure("AuthService.ValidationAccount.InvalidCode");
            }

            _logger.LogInformation("Email validation attempt for: {Email}", decoded.Value.Email);

            var expirationDate = DateTime.Parse(decoded.Value.Expiration);
            var email = decoded.Value.Email;

            user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                return ApplicationResult.Failure("UserNotFound");

            if (user.AccountStatus == AccountStatus.Active)
                return ApplicationResult.Failure("AccountAlreadyConfirmed");

            if (DateTime.Now > expirationDate.AddMinutes(15))
                return ApplicationResult.Failure("ActivationLinkExpired");

            user = User.Update(user, user.Name, user.Email, user.Username, AccountStatus.Active, user.ProfileType);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Account {Email} successfully confirmed", user.Email);
            return ApplicationResult.Success("AccountConfirmedSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during account validation");
            return ApplicationResult.Failure("AnErrorOccurredDuringAccountValidation");
        }
    }
}

