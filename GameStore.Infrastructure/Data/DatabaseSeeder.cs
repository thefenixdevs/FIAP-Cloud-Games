using BCrypt.Net;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameStore.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly GameStoreContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(GameStoreContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminUserAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@gamestore.com";

        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingAdmin != null)
        {
            _logger.LogInformation("Admin user already exists: {Email}", adminEmail);
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var adminUser = new User(adminEmail, "Admin", passwordHash);
        adminUser.SetProfileType(ProfileType.Admin);
        adminUser.ConfirmAccount();

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
    }
}
