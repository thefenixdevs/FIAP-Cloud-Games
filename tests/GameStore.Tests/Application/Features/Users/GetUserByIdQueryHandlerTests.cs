using GameStore.Application.Features.Users.UseCases.GetUserById;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<GetUserByIdQueryHandler>>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new GetUserByIdQueryHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHasher = new TestPasswordHasher();
        var user = User.Register("Test User", "TestUser@Email.com", "TestUsername", "Password@123", passwordHasher, ProfileType.CommonUser);
        user.Id = userId;

        _unitOfWork.Users.GetByIdAsync(userId).Returns(user);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.Id);
        Assert.Equal("Test User", result.Data.Name);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _unitOfWork.Users.GetByIdAsync(userId).Returns((User?)null);

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UserNotFound", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _unitOfWork.Users.GetByIdAsync(userId).Returns(Task.FromException<User?>(new Exception("Database error")));

        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while fetching the user", result.Message);
        Assert.Null(result.Data);
    }
}

