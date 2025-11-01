using GameStore.Infrastructure.Data.Seeders.Abstractions;
using Microsoft.Extensions.Logging;

namespace GameStore.Infrastructure.Data.Seeders;

/// <summary>
/// Orquestra a execução de múltiplos seeders de dados na ordem correta.
/// </summary>
public class DataSeederOrchestrator
{
  private readonly IEnumerable<IDataSeeder> _seeders;
  private readonly ILogger<DataSeederOrchestrator> _logger;

  public DataSeederOrchestrator(IEnumerable<IDataSeeder> seeders, ILogger<DataSeederOrchestrator> logger)
  {
    _seeders = seeders;
    _logger = logger;
  }

  public async Task SeedAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Starting data seeding orchestration");

    foreach (var seeder in OrderSeeders(_seeders))
    {
      var seederName = seeder.GetType().Name;

      try
      {
        _logger.LogInformation("Executing seeder: {SeederName}", seederName);
        await seeder.SeedAsync(cancellationToken);
        _logger.LogInformation("Seeder {SeederName} completed successfully", seederName);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Seeder {SeederName} failed", seederName);
        throw;
      }
    }

    _logger.LogInformation("Data seeding orchestration completed");
  }

  private static IEnumerable<IDataSeeder> OrderSeeders(IEnumerable<IDataSeeder> seeders)
  {
    return seeders
      .Select(seeder => new
      {
        Seeder = seeder,
        Order = seeder is IOrderedDataSeeder orderedSeeder ? orderedSeeder.Order : int.MaxValue
      })
      .OrderBy(entry => entry.Order)
      .Select(entry => entry.Seeder);
  }
}
