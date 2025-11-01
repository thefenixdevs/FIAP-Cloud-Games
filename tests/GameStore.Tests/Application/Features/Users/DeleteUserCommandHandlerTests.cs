using GameStore.Application.Features.Users.UseCases.DeleteUser;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Users;

public class DeleteUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<DeleteUserCommandHandler>>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new DeleteUserCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_DeletesUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHasher = new TestPasswordHasher();
        var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", passwordHasher, ProfileType.CommonUser);
        existingUser.Id = userId;

        var command = new DeleteUserCommand(userId);

        _unitOfWork.Users.GetByIdAsync(userId).Returns(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UserDeletedSuccessfully", result.Message);
        await _unitOfWork.Users.Received(1).GetByIdAsync(userId);
        await _unitOfWork.Users.Received(1).DeleteAsync(userId);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _unitOfWork.Users.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserNotFound", result.Message);
        await _unitOfWork.Users.Received(1).GetByIdAsync(userId);
        await _unitOfWork.Users.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHasher = new TestPasswordHasher();
        var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", passwordHasher, ProfileType.CommonUser);
        existingUser.Id = userId;

        var command = new DeleteUserCommand(userId);

        _unitOfWork.Users.GetByIdAsync(userId).Returns(existingUser);
        _unitOfWork.Users.DeleteAsync(Arg.Any<Guid>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserService.DeleteUserAsync.AnErrorOccurredWhileDeletingTheUser", result.Message);
    }
}

