using GameStore.Application.Features.Auth.DTOs;
using GameStore.Domain.Aggregates.UserAggregate;
using Mapster;

namespace GameStore.Application.Features.Auth.Mappings;

public class AuthMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User -> LoginResponse
        config.NewConfig<User, LoginResponse>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ProfileType, src => src.ProfileType)
            .Map(dest => dest.AccountStatus, src => src.AccountStatus)
            .Map(dest => dest.Token, src => string.Empty); // Token será preenchido manualmente

        // User -> UserResponse
        config.NewConfig<User, UserResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ProfileType, src => src.ProfileType)
            .Map(dest => dest.AccountStatus, src => src.AccountStatus)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}
