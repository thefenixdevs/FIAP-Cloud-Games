using GameStore.Application.Features.Games.DTOs;
using GameStore.Domain.Aggregates.GameAggregate;
using Mapster;

namespace GameStore.Application.Features.Games.Mappings;

public class GameMappingConfig : IRegister
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

        // CreateGameRequest -> Game (usando construtor)
        config.NewConfig<CreateGameRequest, Game>()
            .ConstructUsing(src => new Game(
                src.Title,
                src.Description,
                src.Price,
                src.Genre,
                src.ReleaseDate));

        // UpdateGameRequest -> Game (usando método Update)
        config.NewConfig<UpdateGameRequest, Game>()
            .MapWith(src => new Game("", "", 0, "", null))
            .AfterMapping((src, dest) => dest.Update(
                src.Title,
                src.Description,
                src.Price,
                src.Genre,
                src.ReleaseDate));
    }
}

