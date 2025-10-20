using GameStore.Domain.Aggregates.GameAggregate.Repositories;
using GameStore.Domain.Aggregates.UserAggregate.Repositories;
using GameStore.Domain.SeedWork.Behavior;
using GameStore.Domain.Services.PasswordService;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Data.Seeders;
using GameStore.Infrastructure.Data.Seeders.Abstractions;
using GameStore.Infrastructure.Data.Seeders.Users;
using GameStore.Infrastructure.Repositories.Abstractions;
using GameStore.Infrastructure.Repositories.Games;
using GameStore.Infrastructure.Repositories.Users;
using GameStore.Infrastructure.Services.PasswordServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.Installers.Services;

public static class InfrastructureLayerServices
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
    services.AddScoped<IPasswordService, PasswordService>();

    return services;
  }
}
