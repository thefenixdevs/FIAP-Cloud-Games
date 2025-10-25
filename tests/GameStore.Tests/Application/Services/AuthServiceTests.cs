using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameStore.Tests.Application.Services;

public class AuthServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<IJwtService> _jwtServiceMock;
  private readonly Mock<ILogger<AuthService>> _loggerMock;
  private readonly Mock<IPasswordHasher> _passwordHasherMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly Mock<IEncriptService> _encriptServiceMock;
  private readonly AuthService _authService;

  public AuthServiceTests()
  {
    _userRepositoryMock = new Mock<IUserRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _jwtServiceMock = new Mock<IJwtService>();
    _loggerMock = new Mock<ILogger<AuthService>>();
    _passwordHasherMock = new Mock<IPasswordHasher>();
    _emailServiceMock = new Mock<IEmailService>();
    _encriptServiceMock = new Mock<IEncriptService>();
    _passwordHasherMock.Setup(x => x.Hash(It.IsAny<string>())).Returns<string>(password => $"HASH::{password}");
    _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((hash, password) => hash == $"HASH::{password}");
    _unitOfWorkMock.SetupGet(x => x.Users).Returns(_userRepositoryMock.Object);
    _authService = new AuthService(
        _unitOfWorkMock.Object, 
        _jwtServiceMock.Object, 
        _loggerMock.Object, 
        _passwordHasherMock.Object,
        _emailServiceMock.Object,
        _encriptServiceMock.Object);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsValid()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

    var result = await _authService.RegisterAsync(request);

    Assert.True(result.Success);
    Assert.Equal("UserRegisteredSuccessfully", result.Message);
    Assert.NotNull(result.UserId);
    _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

    var result = await _authService.RegisterAsync(request);

    Assert.False(result.Success);
    Assert.Equal("EmailAlreadyExists", result.Message);
    Assert.Null(result.UserId);
    _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenUsernameAlreadyExists()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(true);

    var result = await _authService.RegisterAsync(request);

    Assert.False(result.Success);
    Assert.Equal("UsernameAlreadyExists", result.Message);
    Assert.Null(result.UserId);
    _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task RegisterAsync_ShouldHashPassword_WhenCreatingUser()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    User? capturedUser = null;
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
        .Callback<User>(user => capturedUser = user)
        .Returns(Task.CompletedTask);
    _unitOfWorkMock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

    await _authService.RegisterAsync(request);

    Assert.NotNull(capturedUser);
    Assert.Equal("HASH::Password123!", capturedUser!.Password.Hash);
    _passwordHasherMock.Verify(x => x.Hash(request.Password), Times.Once);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenExceptionOccurs()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ThrowsAsync(new Exception("Database error"));

    var result = await _authService.RegisterAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.RegisterAsync.AnErrorOccurredDuringRegistration", result.Message);
    Assert.Null(result.UserId);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenPasswordIsWeak()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "weak");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);
    _userRepositoryMock.Setup(x => x.ExistsByUsernameAsync(request.Username)).ReturnsAsync(false);

    var result = await _authService.RegisterAsync(request);

    Assert.False(result.Success);
    Assert.Contains("Password must be at least 8 characters long. (Parameter 'password')", result.Message);
    Assert.Null(result.UserId);
    _passwordHasherMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);
    var expectedToken = "jwt-token";

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);
    _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

    var result = await _authService.LoginAsync(request);

    Assert.True(result.Success);
    Assert.Equal("AuthService.LoginAsync.LoginSuccessful", result.Message);
    Assert.NotNull(result.Response);
    Assert.Equal(user.Id, result.Response.UserId);
    Assert.Equal(user.Username, result.Response.Username);
    Assert.Equal(user.Email.Value, result.Response.Email);
    Assert.Equal(expectedToken, result.Response.Token);
    Assert.Equal(user.ProfileType, result.Response.ProfileType);
    Assert.Equal(user.AccountStatus, result.Response.AccountStatus);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnSuccess_WhenAuthenticatingWithUsername()
  {
    var username = "testuser";
    var request = new LoginRequest(username, "Password123!");
    var user = CreateActiveUser("test@example.com", username, request.Password);
    var expectedToken = "jwt-token";

    _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);
    _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

    var result = await _authService.LoginAsync(request);

    Assert.True(result.Success);
    Assert.Equal("AuthService.LoginAsync.LoginSuccessful", result.Message);
    Assert.NotNull(result.Response);
    Assert.Equal(user.Id, result.Response.UserId);
    Assert.Equal(user.Username, result.Response.Username);
    Assert.Equal(user.Email.Value, result.Response.Email);
    Assert.Equal(expectedToken, result.Response.Token);
    Assert.Equal(user.ProfileType, result.Response.ProfileType);
    Assert.Equal(user.AccountStatus, result.Response.AccountStatus);
    _userRepositoryMock.Verify(x => x.GetByUsernameAsync(username), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.InvalidCredentials", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "WrongPassword1!");
    var user = CreateActiveUser(email, "testuser", "CorrectPassword1!");

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(false);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.InvalidCredentials", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenAccountIsBanned()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);
    user.BanAccount();

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.AccountIsBanned", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenAccountIsPending()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateUser(email, "testuser", request.Password);

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.AccountPendingEmailConfirmation", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenExceptionOccurs()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ThrowsAsync(new Exception("Database error"));

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.AnErrorOccurredDuringLogin", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldCallJwtService_WhenLoginSuccessful()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);
    _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("jwt-token");

    await _authService.LoginAsync(request);

    _jwtServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenAccountIsBlocked()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);
    user.BlockAccount();

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _passwordHasherMock.Setup(x => x.Verify(user.Password.Hash, request.Password)).Returns(true);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("AuthService.LoginAsync.AccountIsBlocked", result.Message);
    Assert.Null(result.Response);
  }

  private static User CreateUser(string email, string username, string password, ProfileType profileType = ProfileType.CommonUser, string? name = null)
  {
    var hasher = new TestPasswordHasher();
    return User.Register(name ?? "Test User", email, username, password, hasher, profileType);
  }

  private static User CreateActiveUser(string email, string username, string password, ProfileType profileType = ProfileType.CommonUser, string? name = null)
  {
    var user = CreateUser(email, username, password, profileType, name);
    user.ConfirmAccount();
    return user;
  }
}
