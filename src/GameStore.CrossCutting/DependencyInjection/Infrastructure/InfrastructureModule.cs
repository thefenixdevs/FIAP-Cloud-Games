using GameStore.Application.Services;
using GameStore.Domain.Repositories;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Data.Seeders;
using GameStore.Infrastructure.Data.Seeders.Abstractions;
using GameStore.Infrastructure.Data.Seeders.Users;
using GameStore.Infrastructure.Repositories.Abstractions;
using GameStore.Infrastructure.Repositories.Games;
using GameStore.Infrastructure.Repositories.Users;
using GameStore.Infrastructure.Security;
using GameStore.Infrastructure.Services.Authentication;
using GameStore.Infrastructure.Services.Email;
using GameStore.Infrastructure.Services.Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.DependencyInjection;

public static class InfrastructureModule
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<GameStoreContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IGameRepository, GameRepository>();

    services.AddScoped<IUnitOfWork, UnitOfWork>();

    services.AddScoped<IDataSeeder, UserSeeder>();
    services.AddScoped<DataSeederOrchestrator>();
    services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
    
    // Serviços de infraestrutura
    services.AddScoped<IJwtService, JwtService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IEncriptService, EncriptService>();

    return services;
  }
}
