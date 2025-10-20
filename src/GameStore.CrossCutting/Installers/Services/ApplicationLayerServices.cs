using GameStore.Application.Common.Options;
using GameStore.Application.Features.Auth;
using GameStore.Application.Features.Auth.Interfaces;
using GameStore.Application.Features.Games;
using GameStore.Application.Features.Games.Interfaces;
using GameStore.Application.Features.Users;
using GameStore.Application.Features.Users.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.Installers.Services;

public static class ApplicationLayerServices
{
  public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
  {
    // Configurar Options
    services.Configure<BaseUrlOptions>(configuration.GetSection(BaseUrlOptions.SectionName));

    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IGameService, GameService>();
    services.AddScoped<IUserService, UserService>();

    return services;
  }
}
