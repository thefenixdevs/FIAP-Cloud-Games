namespace GameStore.Application.Features.Games.Shared;

public record GameResponse(Guid Id, string Title, string Description, decimal Price, string Genre, DateTime? ReleaseDate, DateTime CreatedAt);

