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
}