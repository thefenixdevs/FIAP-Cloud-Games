using System.Linq;
using FluentValidation;
using GameStore.Application.Features.Games.UseCases.UpdateGame;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class UpdateGameCommandValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateGameCommandValidator _validator;

    public UpdateGameCommandValidatorTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());
        
        _validator = new UpdateGameCommandValidator(_unitOfWork);
    }

    [Fact]
    public async Task Validate_ShouldReturnFailure_WhenTitleAlreadyExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Old Game", "Description", 20m, "Action", null);
        existingGame.Id = gameId;
        
        var command = new UpdateGameCommand(gameId, "Existing Game", "Description", 40m, "Puzzle", null);
        
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);
        _unitOfWork.Games.ExistsByTitleAsync("Existing Game").Returns(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == "Title" && e.ErrorMessage == "Games.CreateUpdateGame.TitleAlreadyExists");
        Assert.NotNull(error);
        Assert.Equal("Title", error.PropertyName);
        Assert.Equal("Games.CreateUpdateGame.TitleAlreadyExists", error.ErrorMessage);
    }

    [Fact]
    public async Task Validate_ShouldReturnSuccess_WhenTitleIsUnique()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Old Game", "Description", 20m, "Action", null);
        existingGame.Id = gameId;
        
        var command = new UpdateGameCommand(gameId, "New Game", "Description", 40m, "Puzzle", null);
        
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);
        _unitOfWork.Games.ExistsByTitleAsync("New Game").Returns(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldReturnSuccess_WhenTitleUnchanged()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Same Game", "Description", 20m, "Action", null);
        existingGame.Id = gameId;
        
        var command = new UpdateGameCommand(gameId, "Same Game", "Description", 40m, "Puzzle", null);
        
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        // Não deve verificar se o título existe quando é o mesmo
        await _unitOfWork.Games.DidNotReceive().ExistsByTitleAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Validate_ShouldReturnSuccess_WhenTitleUnchangedCaseInsensitive()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Same Game", "Description", 20m, "Action", null);
        existingGame.Id = gameId;
        
        var command = new UpdateGameCommand(gameId, "same game", "Description", 40m, "Puzzle", null);
        
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        // Não deve verificar se o título existe quando é o mesmo (case-insensitive)
        await _unitOfWork.Games.DidNotReceive().ExistsByTitleAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Validate_ShouldReturnFailure_WhenTitleAlreadyExists_CaseInsensitive()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var existingGame = new Game("Old Game", "Description", 20m, "Action", null);
        existingGame.Id = gameId;
        
        var command = new UpdateGameCommand(gameId, "EXISTING GAME", "Description", 40m, "Puzzle", null);
        
        _unitOfWork.Games.GetByIdAsync(gameId).Returns(existingGame);
        _unitOfWork.Games.ExistsByTitleAsync("EXISTING GAME").Returns(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == "Title" && e.ErrorMessage == "Games.CreateUpdateGame.TitleAlreadyExists");
        Assert.NotNull(error);
    }
}

