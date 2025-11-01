using GameStore.Application.Features.Games.UseCases.GetAllGames;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class GetAllGamesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllGamesQueryHandler> _logger;
    private readonly GetAllGamesQueryHandler _handler;

    public GetAllGamesQueryHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<GetAllGamesQueryHandler>>();

        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());

        _handler = new GetAllGamesQueryHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_ReturnsListOfGameDtos()
    {
        // Arrange
        var game1 = new Game("Game 1", "Just a Game", 10m, "Action", null);
        game1.Id = Guid.NewGuid();
        var game2 = new Game("Game 2", "Another Game", 20m, "RPG", null);
        game2.Id = Guid.NewGuid();
        var games = new List<Game> { game1, game2 };

        _unitOfWork.Games.GetAllAsync().Returns(games);

        var query = new GetAllGamesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var gamesList = result.Data.ToList();
        Assert.Equal(2, gamesList.Count);
        Assert.Contains(gamesList, g => g.Title == "Game 1");
        Assert.Contains(gamesList, g => g.Title == "Game 2");
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoGamesExist()
    {
        // Arrange
        _unitOfWork.Games.GetAllAsync().Returns(new List<Game>());

        var query = new GetAllGamesQuery();

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
        _unitOfWork.Games.GetAllAsync().Returns(Task.FromException<ICollection<Game>>(new Exception("Database error")));

        var query = new GetAllGamesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while fetching games", result.Message);
        Assert.Null(result.Data);
    }
}

