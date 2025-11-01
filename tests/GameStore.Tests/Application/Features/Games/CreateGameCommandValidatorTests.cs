using System.Linq;
using FluentValidation;
using GameStore.Application.Features.Games.UseCases.CreateGame;
using GameStore.Domain.Repositories.Abstractions;
using NSubstitute;
using Xunit;

namespace GameStore.Tests.Application.Features.Games;

public class CreateGameCommandValidatorTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateGameCommandValidator _validator;

    public CreateGameCommandValidatorTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.Games.Returns(Substitute.For<GameStore.Domain.Repositories.IGameRepository>());
        
        _validator = new CreateGameCommandValidator(_unitOfWork);
    }

    [Fact]
    public async Task Validate_ShouldReturnFailure_WhenTitleAlreadyExists()
    {
        // Arrange
        var command = new CreateGameCommand("Existing Game", "Description", 30m, "Strategy", null);
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
        var command = new CreateGameCommand("New Game", "Description", 30m, "Strategy", null);
        _unitOfWork.Games.ExistsByTitleAsync("New Game").Returns(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldReturnSuccess_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateGameCommand(string.Empty, "Description", 30m, "Strategy", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        // Deve falhar por Empty, mas não deve chamar ExistsByTitleAsync
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
        await _unitOfWork.Games.DidNotReceive().ExistsByTitleAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Validate_ShouldReturnFailure_WhenTitleAlreadyExists_CaseInsensitive()
    {
        // Arrange
        var command = new CreateGameCommand("existing game", "Description", 30m, "Strategy", null);
        _unitOfWork.Games.ExistsByTitleAsync("existing game").Returns(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == "Title" && e.ErrorMessage == "Games.CreateUpdateGame.TitleAlreadyExists");
        Assert.NotNull(error);
    }
}

