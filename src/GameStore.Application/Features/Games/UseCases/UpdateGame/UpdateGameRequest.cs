namespace GameStore.Application.Features.Games.UseCases.UpdateGame;

public record UpdateGameRequest(string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate);

