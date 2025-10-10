using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Repositories.Abstractions;
using GameStore.Infrastructure.Repositories.Games;
using GameStore.Infrastructure.Repositories.Users;
using GameStore.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameStore.Tests.Infrastructure.Repositories;

public class UserRepositoryTests : IDisposable
{
  private readonly GameStoreContext _context;
  private readonly UserRepository _repository;
  private readonly UnitOfWork _unitOfWork;

  public UserRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<GameStoreContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new GameStoreContext(options);
    _repository = new UserRepository(_context);
    var gameRepository = new GameRepository(_context);
    _unitOfWork = new UnitOfWork(_context, _repository, gameRepository);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
  }

  [Fact]
  public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByIdAsync(user.Id);

    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
    Assert.Equal(user.Email, result.Email);
    Assert.Equal(user.Username, result.Username);
  }

  [Fact]
  public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
  {
    var nonExistentId = Guid.NewGuid();

    var result = await _repository.GetByIdAsync(nonExistentId);

    Assert.Null(result);
  }

  [Fact]
  public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByEmailAsync("test@example.com");

    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
    Assert.Equal(user.Email, result.Email);
  }

  [Fact]
  public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
  {
    var result = await _repository.GetByEmailAsync("nonexistent@example.com");

    Assert.Null(result);
  }

  [Fact]
  public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
  {
    var user = CreateUser("Test@Example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByEmailAsync("Test@Example.com");

    Assert.NotNull(result);
    Assert.Equal(user.Email, result.Email);
  }

  [Fact]
  public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByUsernameAsync("testuser");

    Assert.NotNull(result);
    Assert.Equal(user.Id, result.Id);
    Assert.Equal(user.Username, result.Username);
  }

  [Fact]
  public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
  {
    var result = await _repository.GetByUsernameAsync("nonexistentuser");

    Assert.Null(result);
  }

  [Fact]
  public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenUserExists()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.ExistsByEmailAsync("test@example.com");

    Assert.True(result);
  }

  [Fact]
  public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
  {
    var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");

    Assert.False(result);
  }

  [Fact]
  public async Task ExistsByUsernameAsync_ShouldReturnTrue_WhenUserExists()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.ExistsByUsernameAsync("testuser");

    Assert.True(result);
  }

  [Fact]
  public async Task ExistsByUsernameAsync_ShouldReturnFalse_WhenUserDoesNotExist()
  {
    var result = await _repository.ExistsByUsernameAsync("nonexistentuser");

    Assert.False(result);
  }

  [Fact]
  public async Task AddAsync_ShouldAddUser_WhenUserIsValid()
  {
    var user = CreateUser("newuser@example.com", "newuser");

    await _repository.AddAsync(user);
    await _unitOfWork.CommitAsync();

    var result = await _context.Users.FindAsync(user.Id);
    Assert.NotNull(result);
    Assert.Equal(user.Email, result.Email);
    Assert.Equal(user.Username, result.Username);
  }

  [Fact]
  public async Task CommitAsync_ShouldPersistChanges_WhenCalled()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _repository.AddAsync(user);

    var countBeforeSave = await _context.Users.CountAsync();
    Assert.Equal(0, countBeforeSave);

    await _unitOfWork.CommitAsync();

    var countAfterSave = await _context.Users.CountAsync();
    Assert.Equal(1, countAfterSave);
  }

  [Fact]
  public async Task AddAsync_ShouldHandleMultipleUsers_WhenAddedSequentially()
  {
    var user1 = CreateUser("user1@example.com", "user1");
    var user2 = CreateUser("user2@example.com", "user2");
    var user3 = CreateUser("user3@example.com", "user3");

    await _repository.AddAsync(user1);
    await _repository.AddAsync(user2);
    await _repository.AddAsync(user3);
    await _unitOfWork.CommitAsync();

    var count = await _context.Users.CountAsync();
    Assert.Equal(3, count);
  }

  [Fact]
  public async Task GetByEmailAsync_ShouldReturnCorrectUser_WhenMultipleUsersExist()
  {
    var user1 = CreateUser("user1@example.com", "user1");
    var user2 = CreateUser("user2@example.com", "user2");
    var user3 = CreateUser("user3@example.com", "user3");
    await _context.Users.AddRangeAsync(user1, user2, user3);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByEmailAsync("user2@example.com");

    Assert.NotNull(result);
    Assert.Equal(user2.Id, result.Id);
    Assert.Equal("user2@example.com", result.Email.Value);
  }

  [Fact]
  public async Task GetByUsernameAsync_ShouldReturnCorrectUser_WhenMultipleUsersExist()
  {
    var user1 = CreateUser("user1@example.com", "user1");
    var user2 = CreateUser("user2@example.com", "user2");
    var user3 = CreateUser("user3@example.com", "user3");
    await _context.Users.AddRangeAsync(user1, user2, user3);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByUsernameAsync("user2");

    Assert.NotNull(result);
    Assert.Equal(user2.Id, result.Id);
    Assert.Equal("user2", result.Username.Value);
  }

  [Fact]
  public async Task AddAsync_ShouldPreserveUserProperties_WhenSaved()
  {
    var user = CreateUser("test@example.com", "testuser", profileType: ProfileType.Admin);
    user.ConfirmAccount();

    await _repository.AddAsync(user);
    await _unitOfWork.CommitAsync();

    var result = await _context.Users.FindAsync(user.Id);
    Assert.NotNull(result);
    Assert.Equal(ProfileType.Admin, result.ProfileType);
    Assert.Equal(AccountStatus.Active, result.AccountStatus);
  }

  [Fact]
  public async Task ExistsByEmailAsync_ShouldNotReturnFalsePositives_WithSimilarEmails()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.ExistsByEmailAsync("test2@example.com");

    Assert.False(result);
  }

  [Fact]
  public async Task ExistsByUsernameAsync_ShouldNotReturnFalsePositives_WithSimilarUsernames()
  {
    var user = CreateUser("test@example.com", "testuser");
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();

    var result = await _repository.ExistsByUsernameAsync("testuser2");

    Assert.False(result);
  }

  private static User CreateUser(string email, string username, string password = "Password123!", ProfileType profileType = ProfileType.CommonUser, string? name = null)
  {
    var hasher = new TestPasswordHasher();
    return User.Register(name ?? "Test User", email, username, password, hasher, profileType);
  }
}
