namespace GameStore.Domain.Services.EmailService;

public interface IEmailTemplateService
{
  string RenderEmailConfirmation(string userName, string confirmationLink);
  string RenderPasswordRecovery(string userName, string recoveryLink);
  string RenderNotification(string userName, string message);
  string RenderPasswordReset(string userName, string resetLink);
  string RenderEmailChangeConfirmation(string userName, string newEmail, string confirmationLink);
  string RenderTemporaryPassword(string userName, string email, string temporaryPassword, string resetLink);
}

