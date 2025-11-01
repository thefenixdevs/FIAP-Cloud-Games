using GameStore.Application.Features.Auth.UseCases.Login;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Domain.ValueObjects;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly LoginCommandHandler _handler;

    static LoginCommandHandlerTests()
    {
        MapsterTestSetup.Initialize();
    }

    public LoginCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _jwtService = Substitute.For<IJwtService>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();
        _passwordHasher = new TestPasswordHasher();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new LoginCommandHandler(
            _unitOfWork,
            _jwtService,
            _logger,
            _passwordHasher);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        var user = CreateActiveUser(email, "testuser", command.Password);
        var expectedToken = "jwt-token";

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        _jwtService.GenerateToken(Arg.Any<User>()).Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.LoginSuccessful", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.UserId);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.Equal(user.Email.Value, result.Data.Email);
        Assert.Equal(expectedToken, result.Data.Token);
        Assert.Equal(user.ProfileType, result.Data.ProfileType);
        Assert.Equal(user.AccountStatus, result.Data.AccountStatus);
        _jwtService.Received(1).GenerateToken(user);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAuthenticatingWithUsername()
    {
        // Arrange
        var username = "testuser";
        var command = new LoginCommand(username, "Password123!");
        var user = CreateActiveUser("test@example.com", username, command.Password);
        var expectedToken = "jwt-token";

        // O handler tenta Email.Create primeiro, que lança ArgumentException para username
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(Task.FromException<User?>(new ArgumentException("Invalid email")));
        _unitOfWork.Users.GetByUsernameAsync(Arg.Any<string>()).Returns(user);
        _jwtService.GenerateToken(Arg.Any<User>()).Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.UserId);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.Equal(user.Email.Value, result.Data.Email);
        Assert.Equal(expectedToken, result.Data.Token);
        Assert.Equal(user.ProfileType, result.Data.ProfileType);
        Assert.Equal(user.AccountStatus, result.Data.AccountStatus);
        _unitOfWork.Users.Received(1).GetByUsernameAsync(username);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.InvalidCredentials", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "WrongPassword1!");
        var user = CreateActiveUser(email, "testuser", "CorrectPassword1!");

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.InvalidCredentials", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsBanned()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        var user = CreateActiveUser(email, "testuser", command.Password);
        user.BanAccount();

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.AccountIsBanned", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsPending()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        var user = CreateUser(email, "testuser", command.Password);

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.AccountPendingEmailConfirmation", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountIsBlocked()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        var user = CreateActiveUser(email, "testuser", command.Password);
        user.BlockAccount();

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.AccountIsBlocked", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var email = "test@example.com";
        var command = new LoginCommand(email, "Password123!");
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(Task.FromException<User?>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.LoginAsync.AnErrorOccurredDuringLogin", result.Message);
        Assert.Null(result.Data);
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

