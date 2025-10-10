using GameStore.Application.Services;
using GameStore.Domain.Security;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.DependencyInjection;

public static class ApplicationModule
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IGameService, GameService>();

    return services;
  }
}
