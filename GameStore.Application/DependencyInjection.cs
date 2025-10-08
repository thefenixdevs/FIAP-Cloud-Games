using GameStore.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IGameService, GameService>();

    return services;
  }
}
