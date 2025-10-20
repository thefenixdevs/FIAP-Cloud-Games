using GameStore.Application.Features.Users.DTOs;
using GameStore.Domain.Aggregates.UserAggregate;
using GameStore.Domain.Aggregates.UserAggregate.Enums;
using Mapster;

namespace GameStore.Application.Features.Users.Mappings;

public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // RegisterRequest -> User (usando método estático User.Register)
        config.NewConfig<RegisterRequest, User>()
            .MapWith(src => User.Register(
                src.Name,
                src.Email,
                src.Username,
                src.Password,
                ProfileType.CommonUser));

        // User -> UserManagementResponse
        config.NewConfig<User, UserManagementResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Username, src => src.Username)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ProfileType, src => src.ProfileType)
            .Map(dest => dest.AccountStatus, src => src.AccountStatus)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}