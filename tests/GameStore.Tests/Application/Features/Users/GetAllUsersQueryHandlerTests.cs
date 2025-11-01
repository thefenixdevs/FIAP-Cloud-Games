using GameStore.Application.Features.Users.UseCases.GetAllUsers;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Users;

public class GetAllUsersQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<GetAllUsersQueryHandler>>();

        _unitOfWork.Users.Returns(Substitute.For<GameStore.Domain.Repositories.IUserRepository>());

        _handler = new GetAllUsersQueryHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ReturnsListOfUsers_WhenUsersExist()
    {
        // Arrange
        var passwordHasher = new TestPasswordHasher();
        var user1 = User.Register("User 1", "TestUser1@email.com", "TestUsername1", "Password@123", passwordHasher, ProfileType.CommonUser);
        user1.Id = Guid.NewGuid();
        var user2 = User.Register("User 2", "TestUser2@email.com", "TestUsername2", "Password@456", passwordHasher, ProfileType.Admin);
        user2.Id = Guid.NewGuid();
        var users = new List<User> { user1, user2 };

        _unitOfWork.Users.GetAllAsync().Returns(users);

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var usersList = result.Data.ToList();
        Assert.Equal(2, usersList.Count);
        Assert.Contains(usersList, u => u.Name == "User 1");
        Assert.Contains(usersList, u => u.Name == "User 2");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _unitOfWork.Users.GetAllAsync().Returns(new List<User>());

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        _unitOfWork.Users.GetAllAsync().Returns(Task.FromException<ICollection<User>>(new Exception("Database error")));

        var query = new GetAllUsersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while fetching users", result.Message);
        Assert.Null(result.Data);
    }
}

