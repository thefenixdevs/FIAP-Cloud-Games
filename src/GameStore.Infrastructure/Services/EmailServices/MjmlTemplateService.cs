using GameStore.Domain.Services.EmailService;
using Mjml.Net;

namespace GameStore.Infrastructure.Services.EmailServices;

public class MjmlTemplateService : IEmailTemplateService
{
  private readonly IMjmlRenderer _mjmlRenderer;
  private readonly string _templatesPath;

  public MjmlTemplateService(IMjmlRenderer mjmlRenderer)
  {
    _mjmlRenderer = mjmlRenderer;
    _templatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailServices", "Templates");
  }

  public string RenderEmailConfirmation(string userName, string confirmationLink)
  {
    var templatePath = Path.Combine(_templatesPath, "EmailConfirmation.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{ConfirmationLink}}", confirmationLink);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }

  public string RenderPasswordRecovery(string userName, string recoveryLink)
  {
    var templatePath = Path.Combine(_templatesPath, "PasswordRecovery.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{RecoveryLink}}", recoveryLink);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }

  public string RenderNotification(string userName, string message)
  {
    var templatePath = Path.Combine(_templatesPath, "Notification.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{Message}}", message);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }

  public string RenderPasswordReset(string userName, string resetLink)
  {
    var templatePath = Path.Combine(_templatesPath, "PasswordReset.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{ResetLink}}", resetLink);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }

  public string RenderEmailChangeConfirmation(string userName, string newEmail, string confirmationLink)
  {
    var templatePath = Path.Combine(_templatesPath, "EmailChangeConfirmation.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{NewEmail}}", newEmail)
      .Replace("{{ConfirmationLink}}", confirmationLink);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }

  public string RenderTemporaryPassword(string userName, string email, string temporaryPassword, string resetLink)
  {
    var templatePath = Path.Combine(_templatesPath, "TemporaryPassword.mjml");
    var mjmlContent = File.ReadAllText(templatePath);

    mjmlContent = mjmlContent
      .Replace("{{UserName}}", userName)
      .Replace("{{Email}}", email)
      .Replace("{{TemporaryPassword}}", temporaryPassword)
      .Replace("{{ResetLink}}", resetLink);

    var result = _mjmlRenderer.Render(mjmlContent);

    if (result.Errors.Any())
    {
      var errors = string.Join(", ", result.Errors.Select(e => e.Error));
      throw new InvalidOperationException($"Erro ao renderizar template MJML: {errors}");
    }

    return result.Html;
  }
}

