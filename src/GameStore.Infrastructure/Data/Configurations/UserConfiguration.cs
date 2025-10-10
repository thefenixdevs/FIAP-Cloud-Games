using GameStore.Domain.Entities;
using GameStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameStore.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");

    builder.HasKey(e => e.Id);

    var emailComparer = new ValueComparer<Email>(
      (left, right) => Equals(left, right),
      value => value == null ? 0 : value.GetHashCode(),
      value => value);

    var usernameComparer = new ValueComparer<Username>(
      (left, right) => Equals(left, right),
      value => value == null ? 0 : value.GetHashCode(),
      value => value);

    var passwordComparer = new ValueComparer<Password>(
      (left, right) => Equals(left, right),
      value => value == null ? 0 : value.GetHashCode(),
      value => value);

    var emailProperty = builder.Property(e => e.Email).IsRequired();
    emailProperty.HasConversion(
      email => email.Value,
      value => Email.Create(value));
    emailProperty.Metadata.SetValueComparer(emailComparer);

    var usernameProperty = builder.Property(e => e.Username).IsRequired();
    usernameProperty.HasConversion(
      username => username.Value,
      value => Username.Create(value));
    usernameProperty.Metadata.SetValueComparer(usernameComparer);

    var passwordProperty = builder.Property(e => e.Password).IsRequired();
    passwordProperty.HasConversion(
      password => password.Hash,
      value => Password.FromHash(value));
    passwordProperty.Metadata.SetValueComparer(passwordComparer);
    passwordProperty.HasColumnName("PasswordHash");

    builder.Property(e => e.Name).IsRequired();
    builder.Property(e => e.AccountStatus).HasConversion<string>().IsRequired();
    builder.Property(e => e.ProfileType).HasConversion<string>().IsRequired();
    builder.HasIndex(e => e.Email).IsUnique();
    builder.HasIndex(e => e.Username).IsUnique();
  }
}
