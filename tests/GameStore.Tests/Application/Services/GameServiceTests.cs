using FluentValidation;
using GameStore.Application.Features.Games.DTOs;
using GameStore.Application.Features.Games;
using Microsoft.Extensions.Logging;
using Moq;
using Mapster;
using GameStore.Domain.Aggregates.GameAggregate;
using GameStore.Domain.Aggregates.GameAggregate.Repositories;
using GameStore.Domain.SeedWork.Behavior;

namespace GameStore.Tests.Application.Services;

public class GameServiceTests
{
  private readonly Mock<IGameRepository> _gameRepositoryMock;
  private readonly Mock<IUnitOfWork> _unitOfWorkMock;
  private readonly Mock<ILogger<GameService>> _loggerMock;
  private readonly Mock<IValidator<CreateGameRequest>> _createGameValidatorMock;
  private readonly Mock<IValidator<UpdateGameRequest>> _updateGameValidatorMock;
  private readonly TypeAdapterConfig _mapperConfig;
  private readonly GameService _gameService;

  public GameServiceTests()
  {
    _gameRepositoryMock = new Mock<IGameRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _loggerMock = new Mock<ILogger<GameService>>();
    _createGameValidatorMock = new Mock<IValidator<CreateGameRequest>>();
    _updateGameValidatorMock = new Mock<IValidator<UpdateGameRequest>>();
    _mapperConfig = new TypeAdapterConfig();
    
    _unitOfWorkMock.SetupGet(x => x.Games).Returns(_gameRepositoryMock.Object);
    
    // Configurar validators para sempre retornar sucesso
    _createGameValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<CreateGameRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    _updateGameValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateGameRequest>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    
    _gameService = new GameService(
      _unitOfWorkMock.Object, 
      _loggerMock.Object, 
      _mapperConfig,
      _createGameValidatorMock.Object,
      _updateGameValidatorMock.Object);
  }

  [Fact]
  public async Task GetGameByIdAsync_ReturnsGameDto_WhenGameExists()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    var game = new Game("Test Game", "Just a Game", 59.99m, "Action", null);
        game.Id = gameId;
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

    // Act
    var result = await _gameService.GetGameByIdAsync(gameId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(gameId, result.Id);
    Assert.Equal("Test Game", result.Title);
  }

  [Fact]
  public async Task GetGameByIdAsync_ReturnsNull_WhenGameDoesNotExist()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

    // Act
    var result = await _gameService.GetGameByIdAsync(gameId);

    // Assert
    Assert.Null(result);
  }

  [Fact]
  public async Task GetAllGamesAsync_ReturnsListOfGameDtos()
  {
    // Arrange
    var game1 = new Game("Game 1", "Just a Game", 10m, "Action", null);
    game1.Id = Guid.NewGuid();
    var game2 = new Game("Game 2", "Another Game", 20m, "RPG", null);
    game2.Id = Guid.NewGuid();
    var games = new List<Game>
    {
        game1,
        game2
    };
    _gameRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

    // Act
    var result = await _gameService.GetAllGamesAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.ToList().Count);
    Assert.Contains(result, g => g.Title == "Game 1");
    Assert.Contains(result, g => g.Title == "Game 2");
  }

  [Fact]
  public async Task CreateGameAsync_CreatesGameAndReturnsDto()
  {
    // Arrange
    var createGameRequest = new CreateGameRequest("New Game", "Just a Game", 30m, "Strategy", null);
    var createdGame = new Game(createGameRequest.Title, createGameRequest.Description, createGameRequest.Price, createGameRequest.Genre, createGameRequest.ReleaseDate);

    _gameRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Game>()));

    //Act
    var result = await _gameService.CreateGameAsync(createGameRequest);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(createGameRequest.Title, result.Game.Title);
    Assert.Equal(createGameRequest.Genre, result.Game.Genre);
    Assert.Equal(createGameRequest.Price, result.Game.Price);
  }

  [Fact]
  public async Task UpdateGameAsync_UpdatesGame_WhenGameExists()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    var updateGameRequest = new UpdateGameRequest("Updated Game", "Just a Game", 40m, "Puzzle", null);

    var existingGame = new Game("Old Game", "Just an Old Game", 20m, "Action", null);
    existingGame.Id = gameId;
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(existingGame);

    // Act
    var result = await _gameService.UpdateGameAsync(gameId, updateGameRequest);

    // Assert
    Assert.True(result.Success);
    Assert.Equal(updateGameRequest.Title, existingGame.Title);
    Assert.Equal(updateGameRequest.Genre, existingGame.Genre);
    Assert.Equal(updateGameRequest.Price, existingGame.Price);
    _unitOfWorkMock.Verify(u => u.Games.UpdateAsync(It.IsAny<Game>()), Times.Once);
  }

  [Fact]
  public async Task UpdateGameAsync_ReturnsFalse_WhenGameDoesNotExist()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    var updateGameRequest = new UpdateGameRequest("Nonexistent Game", "Just a Game", 40m, "Puzzle", null);
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

    // Act
    var result = await _gameService.UpdateGameAsync(gameId, updateGameRequest);

    // Assert
    Assert.False(result.Success);
    _unitOfWorkMock.Verify(u => u.Games.UpdateAsync(It.IsAny<Game>()), Times.Never);
  }

  [Fact]
  public async Task DeleteGameAsync_DeletesGame_WhenGameExists()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    var existingGame = new Game("Game to Delete", "Garbage Game", 9.99m, "None", null);
    existingGame.Id = gameId;
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(existingGame);

    // Act
    var result = await _gameService.DeleteGameAsync(gameId);

    // Assert
    Assert.True(result.Success);
    _gameRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    _unitOfWorkMock.Verify(r => r.Games.DeleteAsync(It.IsAny<Guid>()), Times.Once);
  }

  [Fact]
  public async Task DeleteGameAsync_ReturnsFalse_WhenGameDoesNotExist()
  {
    // Arrange
    var gameId = Guid.NewGuid();
    _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game?)null);

    // Act
    var result = await _gameService.DeleteGameAsync(gameId);

    // Assert
    Assert.False(result.Success);
    _gameRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.Games.DeleteAsync(It.IsAny<Guid>()), Times.Never);
  }
}