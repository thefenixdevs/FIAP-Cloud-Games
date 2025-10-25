using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameStore.Tests.Application.Services;

public class UserServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<ILogger<UserService>> _loggerMock;
  private readonly Mock<IPasswordHasher> _passwordHasherMock;
  private readonly UserService _userService;

  public UserServiceTests()
  {
    _userRepositoryMock = new Mock<IUserRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _loggerMock = new Mock<ILogger<UserService>>();
    _passwordHasherMock = new Mock<IPasswordHasher>();
    _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns<string>(password => $"HASH::{password}");
    _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((hash, password) => hash == $"HASH::{password}");
    _unitOfWorkMock.SetupGet(x => x.Users).Returns(_userRepositoryMock.Object);
    _userService = new UserService(_unitOfWorkMock.Object, _loggerMock.Object, _passwordHasherMock.Object);
  }

  [Fact]
  public async Task GetUserByIdAsync_ReturnsUserDto_WhenUserExists()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = User.Register("Test User", "TestUser@Email.com", "TestUsername", "Password@123", _passwordHasherMock.Object, ProfileType.CommonUser);
    user.Id = userId;
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
    Assert.Equal("Test User", result.Name);
  }

  [Fact]
  public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
  {
    // Arrange
    var userId = Guid.NewGuid();
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task GetAllUsersAsync_ReturnsListOfUserDtos()
  {
    // Arrange
    var user1 = User.Register("User 1", "TestUser1@email.com", "TestUsername1", "Password@123", _passwordHasherMock.Object, ProfileType.CommonUser);
    user1.Id = Guid.NewGuid();
    var user2 = User.Register("User 2", "TestUser2@email.com", "TestUsername2", "Password@456", _passwordHasherMock.Object, ProfileType.Admin);
    user2.Id = Guid.NewGuid();
    var users = new List<User>
    {
        user1,
        user2
    };
    _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

    // Act
    var result = await _userService.GetAllUsersAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.ToList().Count);
    Assert.Contains(result, g => g.Name == "User 1");
    Assert.Contains(result, g => g.Name == "User 2");
  }

  [Fact]
  public async Task CreateUserAsync_CreatesUserAndReturnsDto()
  {
    // Arrange
    var createUserRequest = new CreateUserRequest("New User", "NewUser@email.com".ToLower(), "NewUsername", "Password@123", ProfileType.CommonUser);
    var createdUser = User.Register(createUserRequest.Name, createUserRequest.Email, createUserRequest.Username, createUserRequest.Password, _passwordHasherMock.Object, createUserRequest.ProfileType);

    _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()));

    //Act
    var result = await _userService.CreateUserAsync(createUserRequest);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Success);
    Assert.Equal("UserRegisteredSuccessfully", result.Message);
    Assert.NotNull(result.User);
    Assert.Equal(createUserRequest.Name, result.User.Name);
    Assert.Equal(createUserRequest.Email, result.User.Email);
    Assert.Equal(createUserRequest.Username, result.User.Username);
    Assert.Equal(createUserRequest.ProfileType, result.User.ProfileType);
  }

  [Fact]
  public async Task UpdateUserAsync_UpdatesUser_WhenUserExists()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var updateUserRequest = new UpdateUserRequest("Updated User", "UpdatedUser@email.com".ToLower(), "UpdatedUsername", ProfileType.Admin, AccountStatus.Active);

    var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", _passwordHasherMock.Object, ProfileType.CommonUser);
    existingUser.Id = userId;
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

    // Act
    var result = await _userService.UpdateUserAsync(userId, updateUserRequest);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("UserUpdatedSuccessfully", result.Message);
    Assert.Equal(updateUserRequest.Name, existingUser.Name);
    Assert.Equal(updateUserRequest.Email, existingUser.Email);
    Assert.Equal(updateUserRequest.Username, existingUser.Username);
    Assert.Equal(updateUserRequest.ProfileType, existingUser.ProfileType);
    Assert.Equal(updateUserRequest.AccountStatus, existingUser.AccountStatus);
    _unitOfWorkMock.Verify(u => u.Users.UpdateAsync(It.IsAny<User>()), Times.Once);
  }

  [Fact]
  public async Task UpdateUserAsync_ReturnsFalse_WhenUserDoesNotExist()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var updateUserRequest = new UpdateUserRequest("Updated User", "UpdatedUser@email.com", "UpdatedUsername", ProfileType.Admin, AccountStatus.Active);
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

    // Act
    var result = await _userService.UpdateUserAsync(userId, updateUserRequest);

    // Assert
    Assert.False(result.Success);
    Assert.Equal("UserNotFound", result.Message);
    _unitOfWorkMock.Verify(u => u.Users.UpdateAsync(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserAsync_DeletesUser_WhenUserExists()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", _passwordHasherMock.Object, ProfileType.CommonUser);
    existingUser.Id = userId;
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

    // Act
    var result = await _userService.DeleteUserAsync(userId);

    // Assert
    Assert.True(result.Success);
    Assert.Equal("UserDeletedSuccessfully", result.Message);
    _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    _unitOfWorkMock.Verify(r => r.Users.DeleteAsync(It.IsAny<Guid>()), Times.Once);
  }

  [Fact]
  public async Task DeleteUserAsync_ReturnsFalse_WhenUserDoesNotExist()
  {
    // Arrange
    var userId = Guid.NewGuid();
    _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

    // Act
    var result = await _userService.DeleteUserAsync(userId);

    // Assert
    Assert.False(result.Success);
    Assert.Equal("UserNotFound", result.Message);
    _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.Users.DeleteAsync(It.IsAny<Guid>()), Times.Never);
  }
}