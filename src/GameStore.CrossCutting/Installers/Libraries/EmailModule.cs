using GameStore.Domain.Services.EmailService;
using GameStore.Infrastructure.Services.EmailServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mjml.Net;

namespace GameStore.CrossCutting.Installers.Libraries;

public static class EmailModule
{
  public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Configurar EmailSettings
    services.Configure<EmailSettings>(configuration.GetSection("Email"));

    // Registrar serviços de email
    services.AddSingleton<IMjmlRenderer, MjmlRenderer>();
    services.AddScoped<IEmailTemplateService, MjmlTemplateService>();
    services.AddScoped<IEmailService, EmailService>();

    return services;
  }
}

