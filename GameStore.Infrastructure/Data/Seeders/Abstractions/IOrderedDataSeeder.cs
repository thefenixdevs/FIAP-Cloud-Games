namespace GameStore.Infrastructure.Data.Seeders.Abstractions;

public interface IOrderedDataSeeder : IDataSeeder
{
  int Order { get; }
}
