using GameStore.Domain.Aggregates.GameAggregate;
using GameStore.Infrastructure.Data;
using GameStore.Infrastructure.Repositories.Abstractions;
using GameStore.Infrastructure.Repositories.Games;
using GameStore.Infrastructure.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameStore.Tests.Infrastructure.Repositories;

public class GameRepositoryTests : IDisposable
{
  private readonly GameStoreContext _context;
  private readonly GameRepository _repository;
  private readonly UnitOfWork _unitOfWork;

  public GameRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<GameStoreContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new GameStoreContext(options);
    _repository = new GameRepository(_context);
    var userRepository = new UserRepository(_context);
    _unitOfWork = new UnitOfWork(_context, userRepository, _repository);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
  }

  [Fact]
  public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoGamesExist()
  {
    var result = await _repository.GetAllAsync();

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetAllAsync_ShouldReturnAllGames_WhenGamesExist()
  {
    var game1 = new Game("Game 1", "Description 1", 29.99m, "Action", DateTime.UtcNow);
    var game2 = new Game("Game 2", "Description 2", 39.99m, "Adventure", DateTime.UtcNow);
    await _context.Games.AddRangeAsync(game1, game2);
    await _context.SaveChangesAsync();

    var result = await _repository.GetAllAsync();

    Assert.NotNull(result);
    Assert.Equal(2, result.Count());
  }

  [Fact]
  public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
  {
    var game = new Game("Test Game", "Test Description", 49.99m, "RPG", DateTime.UtcNow);
    await _context.Games.AddAsync(game);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByIdAsync(game.Id);

    Assert.NotNull(result);
    Assert.Equal(game.Id, result.Id);
    Assert.Equal(game.Title, result.Title);
    Assert.Equal(game.Description, result.Description);
    Assert.Equal(game.Price, result.Price);
    Assert.Equal(game.Genre, result.Genre);
  }

  [Fact]
  public async Task GetByIdAsync_ShouldReturnNull_WhenGameDoesNotExist()
  {
    var nonExistentId = Guid.NewGuid();

    var result = await _repository.GetByIdAsync(nonExistentId);

    Assert.Null(result);
  }

  [Fact]
  public async Task AddAsync_ShouldAddGame_WhenGameIsValid()
  {
    var game = new Game("New Game", "New Description", 59.99m, "Strategy", DateTime.UtcNow);

    await _repository.AddAsync(game);
    await _unitOfWork.CommitAsync();

    var result = await _context.Games.FindAsync(game.Id);
    Assert.NotNull(result);
    Assert.Equal(game.Title, result.Title);
  }

  [Fact]
  public async Task UpdateAsync_ShouldUpdateGame_WhenGameExists()
  {
    var game = new Game("Original Title", "Original Description", 29.99m, "Puzzle", DateTime.UtcNow);
    await _context.Games.AddAsync(game);
    await _context.SaveChangesAsync();

    game.Update("Updated Title", "Updated Description", 39.99m, "Arcade", DateTime.UtcNow.AddDays(10));
    await _repository.UpdateAsync(game);
    await _unitOfWork.CommitAsync();

    var result = await _context.Games.FindAsync(game.Id);
    Assert.NotNull(result);
    Assert.Equal("Updated Title", result.Title);
    Assert.Equal("Updated Description", result.Description);
    Assert.Equal(39.99m, result.Price);
    Assert.Equal("Arcade", result.Genre);
  }

  [Fact]
  public async Task DeleteAsync_ShouldRemoveGame_WhenGameExists()
  {
    var game = new Game("Game to Delete", "Description", 19.99m, "Sports", DateTime.UtcNow);
    await _context.Games.AddAsync(game);
    await _context.SaveChangesAsync();

    await _repository.DeleteAsync(game.Id);
    await _unitOfWork.CommitAsync();

    var result = await _context.Games.FindAsync(game.Id);
    Assert.Null(result);
  }

  [Fact]
  public async Task CommitAsync_ShouldPersistChanges_WhenCalled()
  {
    var game = new Game("Test Game", "Test Description", 49.99m, "Action", DateTime.UtcNow);
    await _repository.AddAsync(game);

    var countBeforeSave = await _context.Games.CountAsync();
    Assert.Equal(0, countBeforeSave);

    await _unitOfWork.CommitAsync();

    var countAfterSave = await _context.Games.CountAsync();
    Assert.Equal(1, countAfterSave);
  }

  [Fact]
  public async Task AddAsync_ShouldHandleMultipleGames_WhenAddedSequentially()
  {
    var game1 = new Game("Game 1", "Description 1", 29.99m, "Action", DateTime.UtcNow);
    var game2 = new Game("Game 2", "Description 2", 39.99m, "Adventure", DateTime.UtcNow);
    var game3 = new Game("Game 3", "Description 3", 49.99m, "RPG", DateTime.UtcNow);

    await _repository.AddAsync(game1);
    await _repository.AddAsync(game2);
    await _repository.AddAsync(game3);
    await _unitOfWork.CommitAsync();

    var result = await _repository.GetAllAsync();
    Assert.Equal(3, result.Count());
  }

  [Fact]
  public async Task GetByIdAsync_ShouldReturnCorrectGame_WhenMultipleGamesExist()
  {
    var game1 = new Game("Game 1", "Description 1", 29.99m, "Action", DateTime.UtcNow);
    var game2 = new Game("Game 2", "Description 2", 39.99m, "Adventure", DateTime.UtcNow);
    var game3 = new Game("Game 3", "Description 3", 49.99m, "RPG", DateTime.UtcNow);
    await _context.Games.AddRangeAsync(game1, game2, game3);
    await _context.SaveChangesAsync();

    var result = await _repository.GetByIdAsync(game2.Id);

    Assert.NotNull(result);
    Assert.Equal(game2.Id, result.Id);
    Assert.Equal("Game 2", result.Title);
  }

  [Fact]
  public async Task UpdateAsync_ShouldNotAffectOtherGames_WhenUpdatingOneGame()
  {
    var game1 = new Game("Game 1", "Description 1", 29.99m, "Action", DateTime.UtcNow);
    var game2 = new Game("Game 2", "Description 2", 39.99m, "Adventure", DateTime.UtcNow);
    await _context.Games.AddRangeAsync(game1, game2);
    await _context.SaveChangesAsync();

    game1.Update("Updated Game 1", "Updated Description 1", 19.99m, "Arcade", null);
    await _repository.UpdateAsync(game1);
    await _unitOfWork.CommitAsync();

    var updatedGame1 = await _context.Games.FindAsync(game1.Id);
    var unchangedGame2 = await _context.Games.FindAsync(game2.Id);

    Assert.NotNull(updatedGame1);
    Assert.Equal("Updated Game 1", updatedGame1.Title);
    Assert.NotNull(unchangedGame2);
    Assert.Equal("Game 2", unchangedGame2.Title);
  }
}
