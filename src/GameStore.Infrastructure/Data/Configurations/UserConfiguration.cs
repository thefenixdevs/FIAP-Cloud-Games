using GameStore.Domain.Entities;
using GameStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(e => e.Id);

    var emailProperty = builder.Property(e => e.Email).IsRequired();
    emailProperty.HasConversion(
      email => email.Value,
      value => Email.Create(value));

    builder
      .Property(e => e.Username)
      .IsRequired()
      .HasMaxLength(128)
      .UseCollation("NOCASE");

    var passwordProperty = builder.Property(e => e.Password).IsRequired();
    passwordProperty.HasConversion(
      password => password.Hash,
      value => Password.FromHash(value));
    passwordProperty.HasColumnName("PasswordHash");

    builder.Property(e => e.Name).IsRequired();
    builder.Property(e => e.AccountStatus).HasConversion<string>().IsRequired();
    builder.Property(e => e.ProfileType).HasConversion<string>().IsRequired();
    builder.HasIndex(e => e.Email).IsUnique();
    builder.HasIndex(e => e.Username).IsUnique();
  }
}
