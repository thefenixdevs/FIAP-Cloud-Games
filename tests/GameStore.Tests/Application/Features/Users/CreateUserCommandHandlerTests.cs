using GameStore.Application.Features.Users.UseCases.CreateUser;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IPasswordHasher _passwordHasher;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CreateUserCommandHandler>>();
        _passwordHasher = new TestPasswordHasher();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new CreateUserCommandHandler(_unitOfWork, _logger, _passwordHasher);
    }

    [Fact]
    public async Task Handle_CreatesUserAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateUserCommand("New User", "NewUser@email.com", "NewUsername", "Password@123", ProfileType.CommonUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UserRegisteredSuccessfully", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Name, result.Data.Name);
        Assert.Equal(command.Email.ToLower(), result.Data.Email);
        Assert.Equal(command.Username, result.Data.Username);
        Assert.Equal(command.ProfileType, result.Data.ProfileType);
        await _unitOfWork.Users.Received(1).AddAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_WhenCreatingUser()
    {
        // Arrange
        var command = new CreateUserCommand("New User", "NewUser@email.com", "NewUsername", "Password@123", ProfileType.CommonUser);
        User? capturedUser = null;
        _unitOfWork.Users.When(x => x.AddAsync(Arg.Any<User>())).Do(x => capturedUser = x.Arg<User>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.True(capturedUser!.Password.Verify(command.Password, _passwordHasher));
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var command = new CreateUserCommand("New User", "NewUser@email.com", "NewUsername", "Password@123", ProfileType.CommonUser);
        _unitOfWork.Users.AddAsync(Arg.Any<User>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserService.CreateUserAsync.AnErrorOccurredWhileCreatingTheUser", result.Message);
        Assert.Null(result.Data);
    }
}

