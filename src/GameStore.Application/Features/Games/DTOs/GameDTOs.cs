namespace GameStore.Application.Features.Games.DTOs;

public record CreateGameRequest(string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate);

public record UpdateGameRequest(string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate);

public record GameResponse(Guid Id, string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate, DateTime CreatedAt);
