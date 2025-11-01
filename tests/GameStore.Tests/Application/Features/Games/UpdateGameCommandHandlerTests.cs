using GameStore.Application.Features.Games.UseCases.UpdateGame;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class UpdateGameCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateGameCommandHandler> _logger;
    private readonly UpdateGameCommandHandler _handler;

    public UpdateGameCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<UpdateGameCommandHandler>>();

        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());

        _handler = new UpdateGameCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_UpdatesGame_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Old Game", "Just an Old Game", 20m, "Action", null);
        existingGame.Id = gameId;

        var command = new UpdateGameCommand(gameId, "Updated Game", "Just a Game", 40m, "Puzzle", null);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GameUpdatedSuccessfully", result.Message);
        await _unitOfWork.Games.Received(1).UpdateAsync(Arg.Is<Game>(g =>
            g.Id == gameId &&
            g.Title == command.Title &&
            g.Genre == command.Genre &&
            g.Price == command.Price));
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var command = new UpdateGameCommand(gameId, "Nonexistent Game", "Just a Game", 40m, "Puzzle", null);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns((Game?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameNotFound", result.Message);
        await _unitOfWork.Games.DidNotReceive().UpdateAsync(Arg.Any<Game>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Old Game", "Just an Old Game", 20m, "Action", null);
        existingGame.Id = gameId;

        var command = new UpdateGameCommand(gameId, "Updated Game", "Just a Game", 40m, "Puzzle", null);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);
        _unitOfWork.Games.UpdateAsync(Arg.Any<Game>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameService.UpdateGameAsync.AnErrorOccurredWhileUpdatingTheGame", result.Message);
    }

    [Fact]
    public async Task Handle_UpdatesGame_WhenTitleUnchanged()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Same Game", "Just a Game", 20m, "Action", null);
        existingGame.Id = gameId;

        var command = new UpdateGameCommand(gameId, "Same Game", "Updated Description", 40m, "Puzzle", null);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GameUpdatedSuccessfully", result.Message);
        await _unitOfWork.Games.Received(1).UpdateAsync(Arg.Is<Game>(g =>
            g.Id == gameId &&
            g.Title == command.Title));
        await _unitOfWork.Received(1).CommitAsync();
    }
}

