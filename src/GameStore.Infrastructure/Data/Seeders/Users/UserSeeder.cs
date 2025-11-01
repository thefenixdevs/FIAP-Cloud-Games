using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Security;
using GameStore.Domain.ValueObjects;
using GameStore.Infrastructure.Data.Seeders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameStore.Infrastructure.Data.Seeders.Users;

public class UserSeeder : IOrderedDataSeeder
{
  private const string AdminEmail = "admin@gamestore.com";
  private const string AdminUsername = "Admin";
  private const string AdminName = "Game Store Administrator";
  private const string AdminPassword = "Admin#1234";

  private readonly GameStoreContext _context;
  private readonly ILogger<UserSeeder> _logger;
  private readonly IPasswordHasher _passwordHasher;

  public UserSeeder(GameStoreContext context, ILogger<UserSeeder> logger, IPasswordHasher passwordHasher)
  {
    _context = context;
    _logger = logger;
    _passwordHasher = passwordHasher;
  }

  public int Order => 0;

  public async Task SeedAsync(CancellationToken cancellationToken = default)
  {
    var adminEmail = Email.Create(AdminEmail);

    var existingAdmin = await _context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(user => user.Email == adminEmail, cancellationToken);

    if (existingAdmin is not null)
    {
      _logger.LogInformation("Admin user already exists: {Email}", AdminEmail);
      return;
    }

    var adminUser = User.Register(AdminName, AdminEmail, AdminUsername, AdminPassword, _passwordHasher, ProfileType.Admin);
    adminUser.ConfirmAccount();

    await _context.Users.AddAsync(adminUser, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Admin user created successfully: {Email}", AdminEmail);
  }
}
