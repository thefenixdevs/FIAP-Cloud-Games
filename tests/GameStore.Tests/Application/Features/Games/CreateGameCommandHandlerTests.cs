using GameStore.Application.Features.Games.UseCases.CreateGame;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class CreateGameCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateGameCommandHandler> _logger;
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CreateGameCommandHandler>>();

        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());

        _handler = new CreateGameCommandHandler(_unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_CreatesGameAndReturnsSuccess()
    {
        // Arrange
        var command = new CreateGameCommand("New Game", "Just a Game", 30m, "Strategy", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("GameService.CreateGameAsync.GameCreatedSuccessfully", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(command.Title, result.Data.Title);
        Assert.Equal(command.Genre, result.Data.Genre);
        Assert.Equal(command.Price, result.Data.Price);
        await _unitOfWork.Games.Received(1).AddAsync(Arg.Any<Game>());
        await _unitOfWork.Received(1).CommitAsync();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenExceptionOccurs()
    {
        // Arrange
        var command = new CreateGameCommand("New Game", "Just a Game", 30m, "Strategy", null);
        _unitOfWork.Games.AddAsync(Arg.Any<Game>()).Returns(Task.FromException(new Exception("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GameService.CreateGameAsync.AnErrorOccurredWhileCreatingTheGame", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task Handle_CreatesGame_WhenTitleIsUnique()
    {
        // Arrange
        var command = new CreateGameCommand("Unique Game", "Just a Game", 30m, "Strategy", null);
        _unitOfWork.Games.ExistsByTitleAsync("Unique Game").Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _unitOfWork.Games.Received(1).AddAsync(Arg.Any<Game>());
        await _unitOfWork.Received(1).CommitAsync();
    }
}

