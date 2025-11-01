namespace GameStore.Application.Features.Games.UseCases.CreateGame;

public record CreateGameRequest(string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate);

