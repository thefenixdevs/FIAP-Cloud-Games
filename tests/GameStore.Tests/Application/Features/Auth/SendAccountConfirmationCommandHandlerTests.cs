using GameStore.Application.Features.Auth.UseCases.SendAccountConfirmation;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Auth;

public class SendAccountConfirmationCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendAccountConfirmationCommandHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IEncriptService _encriptService;
    private readonly SendAccountConfirmationCommandHandler _handler;

    public SendAccountConfirmationCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<SendAccountConfirmationCommandHandler>>();
        _emailService = Substitute.For<IEmailService>();
        _encriptService = Substitute.For<IEncriptService>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new SendAccountConfirmationCommandHandler(
            _unitOfWork,
            _logger,
            _emailService,
            _encriptService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailIsValid()
    {
        // Arrange
        var email = "test@example.com";
        var command = new SendAccountConfirmationCommand(email);
        var user = CreateUser(email, "testuser");

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(user);
        _encriptService.EncodeMaskedCode(Arg.Any<string>()).Returns("encoded-code");
        _emailService.TemplateEmailConfirmation(Arg.Any<string>()).Returns("<html>Template</html>");
        _emailService.SendConfirmationEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("AuthService.SendAccountConfirmationAsync.ConfirmationEmailSentSuccessfully", result.Message);
        await _emailService.Received(1).SendConfirmationEmailAsync(
            Arg.Is<string>(e => e == email),
            Arg.Is<string>(s => s == "Confirmação de conta"),
            Arg.Any<string>());
        _encriptService.Received(1).EncodeMaskedCode(Arg.Is<string>(e => e == email));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsEmpty()
    {
        // Arrange
        var command = new SendAccountConfirmationCommand(string.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Users.CreateUpdateUser.EmailIsRequired", result.Message);
        await _emailService.DidNotReceive().SendConfirmationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var email = "test@example.com";
        var command = new SendAccountConfirmationCommand(email);

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserNotFound", result.Message);
        await _emailService.DidNotReceive().SendConfirmationEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var email = "test@example.com";
        var command = new SendAccountConfirmationCommand(email);

        _unitOfWork.Users.GetByEmailAsync(Arg.Any<string>()).Returns(Task.FromException<User?>(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.SendAccountConfirmationAsync.FailedToSendConfirmationEmail", result.Message);
    }

    private static User CreateUser(string email, string username, string password = "Password123!")
    {
        var hasher = new TestPasswordHasher();
        return User.Register("Test User", email, username, password, hasher);
    }
}

