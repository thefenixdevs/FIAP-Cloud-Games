using GameStore.Application.Features.Games.UseCases.GetGameById;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class GetGameByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetGameByIdQueryHandler> _logger;
    private readonly GetGameByIdQueryHandler _handler;

    public GetGameByIdQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<GetGameByIdQueryHandler>>();

        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());

        _handler = new GetGameByIdQueryHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ReturnsGameDto_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game("Test Game", "Just a Game", 59.99m, "Action", null);
        game.Id = gameId;

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(game);

        var query = new GetGameByIdQuery(gameId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(gameId, result.Data.Id);
        Assert.Equal("Test Game", result.Data.Title);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _unitOfWork.Games.GetByIdAsync(gameId).Returns((Game?)null);

        var query = new GetGameByIdQuery(gameId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameNotFound", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(Task.FromException<Game?>(new Exception("Database error")));

        var query = new GetGameByIdQuery(gameId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while fetching the game", result.Message);
        Assert.Null(result.Data);
    }
}

