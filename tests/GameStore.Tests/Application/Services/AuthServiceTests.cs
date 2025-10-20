using FluentValidation;
using GameStore.Application.Features.Auth.DTOs;
using GameStore.Application.Features.Users.DTOs;
using GameStore.Application.Features.Auth;
using GameStore.Application.Features.Auth.Interfaces;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Mapster;
using GameStore.Domain.Aggregates.UserAggregate;
using GameStore.Domain.Aggregates.UserAggregate.Repositories;
using GameStore.Domain.Aggregates.UserAggregate.ValueObjects;
using GameStore.Domain.SeedWork.Behavior;
using GameStore.Domain.Services.EmailService;
using GameStore.Domain.Aggregates.UserAggregate.Enums;

namespace GameStore.Tests.Application.Services;

public class AuthServiceTests
{
  private readonly Mock<IUserRepository> _userRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<IJwtService> _jwtServiceMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly Mock<IConfiguration> _configurationMock;
  private readonly Mock<ILogger<AuthService>> _loggerMock;
  private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
  private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
  private readonly TypeAdapterConfig _mapperConfig;
  private readonly AuthService _authService;

  public AuthServiceTests()
  {
    // Configurar o PasswordService para os testes
    Password.ConfigureService(new TestPasswordService());
    
    _userRepositoryMock = new Mock<IUserRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _jwtServiceMock = new Mock<IJwtService>();
    _emailServiceMock = new Mock<IEmailService>();
    _configurationMock = new Mock<IConfiguration>();
    _loggerMock = new Mock<ILogger<AuthService>>();
    _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
    _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
    _mapperConfig = new TypeAdapterConfig();
    
    _unitOfWorkMock.SetupGet(x => x.Users).Returns(_userRepositoryMock.Object);
    _configurationMock.Setup(x => x["BaseUrl"]).Returns("http://localhost:5000");
    
    // Configurar validators para sempre retornar sucesso
    _registerValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    _loginValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    
    _authService = new AuthService(
      _unitOfWorkMock.Object,
      _jwtServiceMock.Object,
      _emailServiceMock.Object,
      _loggerMock.Object,
      _configurationMock.Object,
      _mapperConfig,
      _registerValidatorMock.Object,
      _loginValidatorMock.Object);
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
    Assert.Contains("User registered successfully", result.Message);
    Assert.NotNull(result.UserId);
    _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    _emailServiceMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
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
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
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
  }

  [Fact]
  public async Task RegisterAsync_ShouldReturnFailure_WhenExceptionOccurs()
  {
    var request = new RegisterRequest("Test User", "test@example.com", "testuser", "Password123!");
    _userRepositoryMock.Setup(x => x.ExistsByEmailAsync(request.Email)).ThrowsAsync(new Exception("Database error"));

    var result = await _authService.RegisterAsync(request);

    Assert.False(result.Success);
    Assert.Equal("An error occurred during registration", result.Message);
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
    Assert.Contains("violação", result.Message);
    Assert.Null(result.UserId);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);
    var expectedToken = "jwt-token";

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
    _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

    var result = await _authService.LoginAsync(request);

    Assert.True(result.Success);
    Assert.Equal("Login successful", result.Message);
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
    _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

    var result = await _authService.LoginAsync(request);

    Assert.True(result.Success);
    Assert.Equal("Login successful", result.Message);
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
    Assert.Equal("Invalid credentials", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "WrongPassword1!");
    var user = CreateActiveUser(email, "testuser", "CorrectPassword1!");

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("Invalid credentials", result.Message);
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

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("Account is banned", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldReturnFailure_WhenAccountIsPending()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateUser(email, "testuser", request.Password);

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("Account pending email confirmation", result.Message);
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
    Assert.Equal("An error occurred during login", result.Message);
    Assert.Null(result.Response);
  }

  [Fact]
  public async Task LoginAsync_ShouldCallJwtService_WhenLoginSuccessful()
  {
    var email = "test@example.com";
    var request = new LoginRequest(email, "Password123!");
    var user = CreateActiveUser(email, "testuser", request.Password);

    _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
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

    var result = await _authService.LoginAsync(request);

    Assert.False(result.Success);
    Assert.Equal("Account is blocked", result.Message);
    Assert.Null(result.Response);
  }

  private static User CreateUser(string email, string username, string password, ProfileType profileType = ProfileType.CommonUser, string? name = null)
  {
    return User.Register(name ?? "Test User", email, username, password, profileType);
  }

  private static User CreateActiveUser(string email, string username, string password, ProfileType profileType = ProfileType.CommonUser, string? name = null)
  {
    var user = CreateUser(email, username, password, profileType, name);
    user.ConfirmAccount();
    return user;
  }
}
