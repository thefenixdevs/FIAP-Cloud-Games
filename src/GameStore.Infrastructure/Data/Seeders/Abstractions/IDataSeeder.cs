using System.Threading;
using System.Threading.Tasks;

namespace GameStore.Infrastructure.Data.Seeders.Abstractions;

public interface IDataSeeder
{
  Task SeedAsync(CancellationToken cancellationToken = default);
}
