namespace GameStore.Application.Services;

public interface IEmailService
{
  Task SendConfirmationEmailAsync(string toEmail, string subject, string body);
  string TemplateEmailConfirmation(string confirmationLink);
}
