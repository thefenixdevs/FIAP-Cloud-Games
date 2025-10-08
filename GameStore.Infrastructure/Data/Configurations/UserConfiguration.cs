using GameStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(e => e.Id);
    builder.Property(e => e.Name).IsRequired();
    builder.HasIndex(e => e.Email).IsUnique();
    builder.HasIndex(e => e.Username).IsUnique();
    builder.Property(e => e.Email).IsRequired();
    builder.Property(e => e.PasswordHash).IsRequired();
    builder.Property(e => e.AccountStatus).HasConversion<string>().IsRequired();

    builder.HasIndex(e => e.Username).IsUnique();
    builder.HasIndex(e => e.Email).IsUnique();


    }
}
