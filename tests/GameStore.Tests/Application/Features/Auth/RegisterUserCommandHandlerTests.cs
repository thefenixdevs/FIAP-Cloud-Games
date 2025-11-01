using GameStore.Application.Common.Exceptions;
using GameStore.Application.Features.Auth.UseCases.RegisterUser;
using GameStore.Application.Services;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IEncriptService _encriptService;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();
        _passwordHasher = new TestPasswordHasher();
        _emailService = Substitute.For<IEmailService>();
        _encriptService = Substitute.For<IEncriptService>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new RegisterUserCommandHandler(
            _unitOfWork,
            _logger,
            _passwordHasher,
            _emailService,
            _encriptService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand("Test User", "test@example.com", "testuser", "Password123!");
        _encriptService.EncodeMaskedCode(Arg.Any<string>()).Returns("encoded-code");
        _emailService.TemplateEmailConfirmation(Arg.Any<string>()).Returns("<html>Template</html>");
        _emailService.SendConfirmationEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UserRegisteredSuccessfully", result.Message);
        Assert.NotNull(result.Data);
        await _unitOfWork.Users.Received(1).AddAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).CommitAsync();
        await _emailService.Received(1).SendConfirmationEmailAsync(
            Arg.Is<string>(e => e == command.Email),
            Arg.Is<string>(s => s == "Confirmação de conta"),
            Arg.Any<string>());
        _encriptService.Received(1).EncodeMaskedCode(Arg.Is<string>(e => e == command.Email));
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WhenCreatingUser()
    {
        // Arrange
        var command = new RegisterUserCommand("Test User", "test@example.com", "testuser", "Password123!");
        User? capturedUser = null;
        _unitOfWork.Users.When(x => x.AddAsync(Arg.Any<User>())).Do(x => capturedUser = x.Arg<User>());
        _encriptService.EncodeMaskedCode(Arg.Any<string>()).Returns("encoded-code");
        _emailService.TemplateEmailConfirmation(Arg.Any<string>()).Returns("<html>Template</html>");
        _emailService.SendConfirmationEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.True(capturedUser!.Password.Verify(command.Password, _passwordHasher));
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenPasswordIsWeak()
    {
        // Arrange
        var command = new RegisterUserCommand("Test User", "test@example.com", "testuser", "weak");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Message));
        await _unitOfWork.Users.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExceptionOccurs()
    {
        // Arrange
        var command = new RegisterUserCommand("Test User", "test@example.com", "testuser", "Password123!");
        _unitOfWork.Users.AddAsync(Arg.Any<User>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AuthService.RegisterAsync.AnErrorOccurredDuringRegistration", result.Message);
    }
}

