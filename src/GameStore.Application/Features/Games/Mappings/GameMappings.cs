using GameStore.Application.Features.Games.Shared;
using GameStore.Domain.Entities;
using Mapster;

namespace GameStore.Application.Features.Games.Mappings;

/// <summary>
/// Configurações de mapeamento para entidade Game
/// </summary>
public sealed class GameMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Game -> GameResponse
        config.NewConfig<Game, GameResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Genre, src => src.Genre)
            .Map(dest => dest.ReleaseDate, src => src.ReleaseDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}
