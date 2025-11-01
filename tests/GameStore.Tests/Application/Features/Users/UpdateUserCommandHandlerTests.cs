using GameStore.Application.Features.Users.UseCases.UpdateUser;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<UpdateUserCommandHandler>>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new UpdateUserCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_UpdatesUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHasher = new TestPasswordHasher();
        var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", passwordHasher, ProfileType.CommonUser);
        existingUser.Id = userId;

        var command = new UpdateUserCommand(
            userId,
            "Updated User",
            "UpdatedUser@email.com",
            "UpdatedUsername",
            ProfileType.Admin,
            AccountStatus.Active);

        _unitOfWork.Users.GetByIdAsync(userId).Returns(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UserUpdatedSuccessfully", result.Message);
        
        // Verificar que UpdateAsync foi chamado com o objeto modificado
        await _unitOfWork.Users.Received(1).UpdateAsync(Arg.Any<User>());
        await _unitOfWork.Received(1).CommitAsync();
        
        // Verificar que o objeto foi modificado corretamente (User.Update modifica o objeto existente)
        Assert.Equal(command.Name, existingUser.Name);
        // Email é normalizado para lowercase no Email.Create()
        Assert.Equal(command.Email.ToLowerInvariant(), existingUser.Email.Value);
        Assert.Equal(command.Username, existingUser.Username);
        Assert.Equal(command.ProfileType, existingUser.ProfileType);
        Assert.Equal(command.AccountStatus, existingUser.AccountStatus);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserCommand(
            userId,
            "Updated User",
            "UpdatedUser@email.com",
            "UpdatedUsername",
            ProfileType.Admin,
            AccountStatus.Active);

        _unitOfWork.Users.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserNotFound", result.Message);
        await _unitOfWork.Users.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHasher = new TestPasswordHasher();
        var existingUser = User.Register("Old User", "OldUser@email.com", "OldUsername", "Password@123", passwordHasher, ProfileType.CommonUser);
        existingUser.Id = userId;

        var command = new UpdateUserCommand(
            userId,
            "Updated User",
            "UpdatedUser@email.com",
            "UpdatedUsername",
            ProfileType.Admin,
            AccountStatus.Active);

        _unitOfWork.Users.GetByIdAsync(userId).Returns(existingUser);
        _unitOfWork.Users.UpdateAsync(Arg.Any<User>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserService.UpdateUserAsync.AnErrorOccurredWhileUpdatingTheUser", result.Message);
    }
}

