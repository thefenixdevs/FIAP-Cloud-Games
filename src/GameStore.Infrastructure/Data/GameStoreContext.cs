using Microsoft.EntityFrameworkCore;
using GameStore.Domain.Aggregates.UserAggregate;
using GameStore.Domain.Aggregates.GameAggregate;

namespace GameStore.Infrastructure.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Game> Games { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    //Aplica todas as configurações de entidade do assembly atual
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameStoreContext).Assembly);
  }
}
