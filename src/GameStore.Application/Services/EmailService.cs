using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services
{
  public class EmailService : IEmailService
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
      _configuration = configuration;
      _logger = logger;
    }    

    public async Task SendConfirmationEmailAsync(string toEmail, string subject, string body)
    {
      var SmtpServer = _configuration["Email:SmtpServer"] ?? throw new InvalidOperationException("Smt server not configured");
      var SmtpPort = _configuration["Email:SmtpPort"] ?? throw new InvalidOperationException("Smt port not configured");
      var SmtpUser = _configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("Smt user not configured");
      var SmtpPass = _configuration["Email:SmtpPass"] ?? throw new InvalidOperationException("Smt pass not configured");

      using var client = new SmtpClient(SmtpServer)
      {
        Port = int.Parse(SmtpPort),
        Credentials = new NetworkCredential(SmtpUser, SmtpPass),
        EnableSsl = true
      };

      var mail = new MailMessage
      {
        From = new MailAddress(SmtpUser, "Suporte - The Fenix Devs"),
        Subject = subject,
        Body = body,
        IsBodyHtml = true
      };

      mail.To.Add(toEmail);

      await client.SendMailAsync(mail);
    }

    public string TemplateEmailConfirmation(string confirmationLink)
    {
      var htmlBody = $@"
        <!DOCTYPE html>
        <html lang='pt-br'>
        <head>
          <meta charset='UTF-8'>
          <meta name='viewport' content='width=device-width, initial-scale=1.0'>
          <title>Confirmação de Conta</title>
        </head>
        <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
          <table width='100%' border='0' cellspacing='0' cellpadding='0'>
            <tr>
              <td align='center' style='padding: 40px 0; background-color: #ffffff;'>
                <!-- Header com logo -->
                <img src='https://www.pngplay.com/wp-content/uploads/8/Phoenix-Fire-Transparent-Free-PNG.png' alt='Logo' width='120' style='display:block; margin:auto;' />
              </td>
            </tr>

            <tr>
              <td align='center' style='padding: 40px 20px; background-color: #ffffff;'>
                <!-- Texto principal -->
                <h2 style='color:#333333;'>Confirmação de Conta</h2>
                <p style='font-size:16px; color:#555555; max-width:500px;'>
                  Olá! Clique no botão abaixo para confirmar sua conta.
                  <br/><br/>
                  Este link é válido por <strong>15 minutos</strong>.
                </p>

                <!-- Botão -->
                <a href='{confirmationLink}' 
                   style='display:inline-block; padding:12px 24px; margin-top:20px; 
                          background-color:#007BFF; color:#ffffff; text-decoration:none; 
                          border-radius:6px; font-weight:bold;'>
                  Confirmar Conta
                </a>
              </td>
            </tr>

            <tr>
              <td align='center' style='padding: 20px; background-color: #f4f4f4; color: #999999; font-size: 12px;'>
                © {DateTime.Now.Year} GameStore. Todos os direitos reservados.
              </td>
            </tr>
          </table>
        </body>
        </html>";

      return htmlBody;
    }
  }
}
