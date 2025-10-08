using GameStore.Domain.Repositories;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Data.Seeders;
using GameStore.Infrastructure.Data.Seeders.Abstractions;
using GameStore.Infrastructure.Data.Seeders.Users;
using GameStore.Infrastructure.Repositories.Abstractions;
using GameStore.Infrastructure.Repositories.Games;
using GameStore.Infrastructure.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    // Database Context
    services.AddDbContext<GameStoreContext>(options =>
        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IGameRepository, GameRepository>();

    // Unit of Work
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Data Seeders
    services.AddScoped<IDataSeeder, UserSeeder>();
    services.AddScoped<DataSeederOrchestrator>();

    return services;
  }
}
