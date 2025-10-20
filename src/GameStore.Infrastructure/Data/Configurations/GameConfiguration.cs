using GameStore.Domain.Aggregates.GameAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Data.Configurations;

public class GameConfiguration:IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Price).HasPrecision(10, 2);
        builder.Property(e => e.Genre).IsRequired();
        builder.Property(e => e.ReleaseDate);

        builder.HasIndex(e => e.Title).IsUnique();
        builder.HasIndex(e => e.Genre);

    }
}
