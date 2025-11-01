using GameStore.Infrastructure.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameStore.Infrastructure.Data.Initialization;

public static class DatabaseInitializationExtensions
{
  public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
  {
    using var scope = services.CreateScope();
    var scopedProvider = scope.ServiceProvider;

    var context = scopedProvider.GetRequiredService<GameStoreContext>();
    var loggerFactory = scopedProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DatabaseInitializer");

    logger.LogInformation("Applying database migrations...");
    await context.Database.MigrateAsync(cancellationToken);
    logger.LogInformation("Database migrations applied successfully");

    logger.LogInformation("Starting database seeding...");
    var orchestrator = scopedProvider.GetRequiredService<DataSeederOrchestrator>();
    await orchestrator.SeedAsync(cancellationToken);
    logger.LogInformation("Database seeding completed successfully");
  }
}
