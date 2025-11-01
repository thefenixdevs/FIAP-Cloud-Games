using GameStore.Application.Features.Games.UseCases.DeleteGame;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class DeleteGameCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGameCommandHandler> _logger;
    private readonly DeleteGameCommandHandler _handler;

    public DeleteGameCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<DeleteGameCommandHandler>>();

        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());

        _handler = new DeleteGameCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_DeletesGame_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Game to Delete", "Garbage Game", 0m, "None", null);
        existingGame.Id = gameId;

        var command = new DeleteGameCommand(gameId);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GameService.DeleteGameAsync.GameDeletedSuccessfully", result.Message);
        await _unitOfWork.Games.Received(1).GetByIdAsync(gameId);
        await _unitOfWork.Games.Received(1).DeleteAsync(gameId);
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var command = new DeleteGameCommand(gameId);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns((Game?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameNotFound", result.Message);
        await _unitOfWork.Games.Received(1).GetByIdAsync(gameId);
        await _unitOfWork.Games.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Game to Delete", "Garbage Game", 0m, "None", null);
        existingGame.Id = gameId;

        var command = new DeleteGameCommand(gameId);

        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);
        _unitOfWork.Games.DeleteAsync(Arg.Any<Guid>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameService.DeleteGameAsync.AnErrorOccurredWhileDeletingTheGame", result.Message);
    }
}

