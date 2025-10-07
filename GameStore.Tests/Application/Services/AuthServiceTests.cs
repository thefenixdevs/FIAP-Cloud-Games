using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameStore.Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_userRepositoryMock.Object, _jwtServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsValid()
    {
        var request = new RegisterRequest("test@example.com", "testuser", "Password123");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));

        var result = await _authService.RegisterAsync(request);

        Assert.True(result.Success);
        Assert.Equal("User registered successfully", result.Message);
        Assert.NotNull(result.UserId);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequest("test@example.com", "testuser", "Password123");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Email already exists", result.Message);
        Assert.Null(result.UserId);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenUsernameAlreadyExists()
    {
        var request = new RegisterRequest("test@example.com", "testuser", "Password123");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(true);

        var result = await _authService.RegisterAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Username already exists", result.Message);
        Assert.Null(result.UserId);
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPassword_WhenCreatingUser()
    {
        var request = new RegisterRequest("test@example.com", "testuser", "Password123");
        User? capturedUser = null;
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user)
            .Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));

        await _authService.RegisterAsync(request);

        Assert.NotNull(capturedUser);
        Assert.NotEqual(request.Password, capturedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        var request = new RegisterRequest("test@example.com", "testuser", "Password123");
        _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ThrowsAsync(new Exception("Database error"));

        var result = await _authService.RegisterAsync(request);

        Assert.False(result.Success);
        Assert.Equal("An error occurred during registration", result.Message);
        Assert.Null(result.UserId);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User("test@example.com", "testuser", passwordHash);
        user.ConfirmAccount();
        var expectedToken = "jwt-token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

        var result = await _authService.LoginAsync(request);

        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.NotNull(result.Response);
        Assert.Equal(user.Id, result.Response.UserId);
        Assert.Equal(user.Username, result.Response.Username);
        Assert.Equal(user.Email, result.Response.Email);
        Assert.Equal(expectedToken, result.Response.Token);
        Assert.Equal(user.ProfileType, result.Response.ProfileType);
        Assert.Equal(user.AccountStatus, result.Response.AccountStatus);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        var result = await _authService.LoginAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest("test@example.com", "WrongPassword");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User("test@example.com", "testuser", passwordHash);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenAccountIsBanned()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User("test@example.com", "testuser", passwordHash);
        user.BanAccount();

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        var result = await _authService.LoginAsync(request);

        Assert.False(result.Success);
        Assert.Equal("Account is banned", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenAccountIsPending()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User("test@example.com", "testuser", passwordHash);
        var expectedToken = "jwt-token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

        var result = await _authService.LoginAsync(request);

        Assert.True(result.Success);
        Assert.Equal("Login successful", result.Message);
        Assert.NotNull(result.Response);
        Assert.Equal(AccountStatus.Pending, result.Response.AccountStatus);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenExceptionOccurs()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ThrowsAsync(new Exception("Database error"));

        var result = await _authService.LoginAsync(request);

        Assert.False(result.Success);
        Assert.Equal("An error occurred during login", result.Message);
        Assert.Null(result.Response);
    }

    [Fact]
    public async Task LoginAsync_ShouldCallJwtService_WhenLoginSuccessful()
    {
        var request = new LoginRequest("test@example.com", "Password123");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User("test@example.com", "testuser", passwordHash);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("jwt-token");

        await _authService.LoginAsync(request);

        _jwtServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    }
}
