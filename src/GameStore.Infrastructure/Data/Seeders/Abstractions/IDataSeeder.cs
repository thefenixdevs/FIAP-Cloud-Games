namespace GameStore.Infrastructure.Data.Seeders.Abstractions;

public interface IDataSeeder
{
  Task SeedAsync(CancellationToken cancellationToken = default);
}
