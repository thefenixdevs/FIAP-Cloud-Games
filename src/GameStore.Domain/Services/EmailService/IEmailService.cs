namespace GameStore.Domain.Services.EmailService;

public interface IEmailService
{
  Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink);
  Task SendPasswordRecoveryAsync(string toEmail, string toName, string recoveryLink);
  Task SendNotificationAsync(string toEmail, string toName, string subject, string message);
  Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
  Task SendEmailChangeConfirmationAsync(string toEmail, string toName, string newEmail, string confirmationLink);
  Task SendTemporaryPasswordAsync(string toEmail, string toName, string email, string temporaryPassword, string resetLink);
}

