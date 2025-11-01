using GameStore.Application.Features.Auth.UseCases.ValidationAccount;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GameStore.Tests.Application.Features.Auth;

public class ValidationAccountCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ValidationAccountCommandHandler> _logger;
    private readonly IEncriptService _encriptService;
    private readonly ValidationAccountCommandHandler _handler;

    public ValidationAccountCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ValidationAccountCommandHandler>>();
        _encriptService = Substitute.For<IEncriptService>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new ValidationAccountCommandHandler(
            _unitOfWork,
            _logger,
            _encriptService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCodeIsValid()
    {
        // Arrange
        var email = "test@example.com";
        var expiration = DateTime.Now.AddHours(1).ToString("O");
        var code = "encoded-code";
        var command = new ValidationAccountCommand(code);
        var user = CreatePendingUser(email, "testuser");

        _encriptService.DecodeMaskedCode(code).Returns((email, expiration));
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AccountConfirmedSuccessfully", result.Message);
        await _unitOfWork.Users.Received(1).UpdateAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeIsEmpty()
    {
        // Arrange
        var command = new ValidationAccountCommand(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.ValidationAccount.InvalidCode", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeIsInvalid()
    {
        // Arrange
        var code = "invalid-code";
        var command = new ValidationAccountCommand(code);

        _encriptService.DecodeMaskedCode(code).Returns((ValueTuple<string, string>?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.ValidationAccount.InvalidCode", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var email = "test@example.com";
        var expiration = DateTime.Now.AddHours(1).ToString("O");
        var code = "encoded-code";
        var command = new ValidationAccountCommand(code);

        _encriptService.DecodeMaskedCode(code).Returns((email, expiration));
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserNotFound", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountAlreadyConfirmed()
    {
        // Arrange
        var email = "test@example.com";
        var expiration = DateTime.Now.AddHours(1).ToString("O");
        var code = "encoded-code";
        var command = new ValidationAccountCommand(code);
        var user = CreatePendingUser(email, "testuser");
        user.ConfirmAccount();

        _encriptService.DecodeMaskedCode(code).Returns((email, expiration));
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AccountAlreadyConfirmed", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenLinkIsExpired()
    {
        // Arrange
        var email = "test@example.com";
        var expiration = DateTime.Now.AddMinutes(-20).ToString("O");
        var code = "encoded-code";
        var command = new ValidationAccountCommand(code);
        var user = CreatePendingUser(email, "testuser");

        _encriptService.DecodeMaskedCode(code).Returns((email, expiration));
        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("ActivationLinkExpired", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var code = "encoded-code";
        var command = new ValidationAccountCommand(code);

        _encriptService.When(x => x.DecodeMaskedCode(code)).Throw(new Exception("Decode error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AnErrorOccurredDuringAccountValidation", result.Message);
    }

    private static User CreatePendingUser(string email, string username, string password = "Password123!")
    {
        var hasher = new TestPasswordHasher();
        return User.Register("Test User", email, username, password, hasher);
    }
}

