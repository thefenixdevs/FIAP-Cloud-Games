using GameStore.Application.Features.Auth.UseCases.Login;
using GameStore.Application.Features.Users.Shared;
using GameStore.Domain.Entities;
using Mapster;

namespace GameStore.Application.Features.Users.Mappings;

/// <summary>
/// Configurações de mapeamento para entidade User
/// </summary>
public sealed class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User -> UserManagementResponse
        config.NewConfig<User, UserManagementResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email.Value) // Value Object mapping
            .Map(dest => dest.ProfileType, src => src.ProfileType)
            .Map(dest => dest.AccountStatus, src => src.AccountStatus)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // User -> LoginResponse (sem Token, será adicionado no handler)
        config.NewConfig<User, LoginResponse>()
            .Map(dest => dest.UserId, src => src.Id)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.Token, src => string.Empty) // Token será sobrescrito no handler
            .Map(dest => dest.ProfileType, src => src.ProfileType)
            .Map(dest => dest.AccountStatus, src => src.AccountStatus);
    }
}
