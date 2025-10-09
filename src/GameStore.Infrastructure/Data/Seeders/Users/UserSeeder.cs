using System.Threading;
using System.Threading.Tasks;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Data.Seeders.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameStore.Infrastructure.Data.Seeders.Users;

public class UserSeeder : IOrderedDataSeeder
{
  private const string AdminEmail = "admin@gamestore.com";
  private const string AdminPassword = "admin123";

  private readonly GameStoreContext _context;
  private readonly ILogger<UserSeeder> _logger;

  public UserSeeder(GameStoreContext context, ILogger<UserSeeder> logger)
  {
    _context = context;
    _logger = logger;
  }

  public int Order => 0;

  public async Task SeedAsync(CancellationToken cancellationToken = default)
  {
    var existingAdmin = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(user => user.Email == AdminEmail, cancellationToken);

    if (existingAdmin is not null)
    {
      _logger.LogInformation("Admin user already exists: {Email}", AdminEmail);
      return;
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(AdminPassword);

    var adminUser = new User(AdminEmail, "Admin", passwordHash);
    adminUser.SetProfileType(ProfileType.Admin);
    adminUser.ConfirmAccount();

    await _context.Users.AddAsync(adminUser, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Admin user created successfully: {Email}", AdminEmail);
  }
}
