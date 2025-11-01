using GameStore.Application.Common.Results;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Features.Auth.UseCases.SendAccountConfirmation;

public sealed class SendAccountConfirmationCommandHandler : IRequestHandler<SendAccountConfirmationCommand, ApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendAccountConfirmationCommandHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IEncriptService _encriptService;

    public SendAccountConfirmationCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<SendAccountConfirmationCommandHandler> logger,
        IEmailService emailService,
        IEncriptService encriptService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailService = emailService;
        _encriptService = encriptService;
    }

    public async ValueTask<ApplicationResult> Handle(SendAccountConfirmationCommand request, CancellationToken cancellationToken)
    {
        User? user = null;

        if (string.IsNullOrWhiteSpace(request.Email))
            return ApplicationResult.Failure("Users.CreateUpdateUser.EmailIsRequired");

        try
        {
            user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
                return ApplicationResult.Failure("UserNotFound");

            var masked = _encriptService.EncodeMaskedCode(request.Email);
            var confirmationLink = $"https://localhost:7055/api/Auth/ValidationAccount?code={masked}";
            var htmlBody = _emailService.TemplateEmailConfirmation(confirmationLink);

            await _emailService.SendConfirmationEmailAsync(
                user.Email,
                "Confirmação de conta",
                htmlBody
            );

            _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
            return ApplicationResult.Success("AuthService.SendAccountConfirmationAsync.ConfirmationEmailSentSuccessfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending confirmation e-mail for {Email}", request.Email);
            return ApplicationResult.Failure("AuthService.SendAccountConfirmationAsync.FailedToSendConfirmationEmail");
        }
    }
}

