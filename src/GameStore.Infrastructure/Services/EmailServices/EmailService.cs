using GameStore.Domain.Services.EmailService;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GameStore.Infrastructure.Services.EmailServices;

public class EmailService : IEmailService
{
  private readonly EmailSettings _emailSettings;
  private readonly IEmailTemplateService _templateService;
  private readonly ILogger<EmailService> _logger;

  public EmailService(
    IOptions<EmailSettings> emailSettings,
    IEmailTemplateService templateService,
    ILogger<EmailService> logger)
  {
    _emailSettings = emailSettings.Value;
    _templateService = templateService;
    _logger = logger;
  }

  public async Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink)
  {
    try
    {
      var htmlBody = _templateService.RenderEmailConfirmation(toName, confirmationLink);
      await SendEmailAsync(toEmail, toName, "Confirme seu Email - Game Store", htmlBody);
      _logger.LogInformation("Email de confirmação enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de confirmação para {Email}", toEmail);
      throw;
    }
  }

  public async Task SendPasswordRecoveryAsync(string toEmail, string toName, string recoveryLink)
  {
    try
    {
      var htmlBody = _templateService.RenderPasswordRecovery(toName, recoveryLink);
      await SendEmailAsync(toEmail, toName, "Recuperação de Senha - Game Store", htmlBody);
      _logger.LogInformation("Email de recuperação de senha enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de recuperação de senha para {Email}", toEmail);
      throw;
    }
  }

  public async Task SendNotificationAsync(string toEmail, string toName, string subject, string message)
  {
    try
    {
      var htmlBody = _templateService.RenderNotification(toName, message);
      await SendEmailAsync(toEmail, toName, subject, htmlBody);
      _logger.LogInformation("Email de notificação enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de notificação para {Email}", toEmail);
      throw;
    }
  }

  public async Task SendPasswordResetAsync(string toEmail, string toName, string resetLink)
  {
    try
    {
      var htmlBody = _templateService.RenderPasswordReset(toName, resetLink);
      await SendEmailAsync(toEmail, toName, "Redefinir Senha - Game Store", htmlBody);
      _logger.LogInformation("Email de reset de senha enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de reset de senha para {Email}", toEmail);
      throw;
    }
  }

  public async Task SendEmailChangeConfirmationAsync(string toEmail, string toName, string newEmail, string confirmationLink)
  {
    try
    {
      var htmlBody = _templateService.RenderEmailChangeConfirmation(toName, newEmail, confirmationLink);
      await SendEmailAsync(toEmail, toName, "Confirme a Troca de E-mail - Game Store", htmlBody);
      _logger.LogInformation("Email de confirmação de troca de e-mail enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de confirmação de troca de e-mail para {Email}", toEmail);
      throw;
    }
  }

  public async Task SendTemporaryPasswordAsync(string toEmail, string toName, string email, string temporaryPassword, string resetLink)
  {
    try
    {
      var htmlBody = _templateService.RenderTemporaryPassword(toName, email, temporaryPassword, resetLink);
      await SendEmailAsync(toEmail, toName, "Conta Criada - Senha Temporária - Game Store", htmlBody);
      _logger.LogInformation("Email de senha temporária enviado para {Email}", toEmail);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao enviar email de senha temporária para {Email}", toEmail);
      throw;
    }
  }

  private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
  {
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
    message.To.Add(new MailboxAddress(toName, toEmail));
    message.Subject = subject;

    var bodyBuilder = new BodyBuilder
    {
      HtmlBody = htmlBody
    };

    message.Body = bodyBuilder.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
  }
}

