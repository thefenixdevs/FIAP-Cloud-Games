using GameStore.Domain.Aggregates.UserAggregate;

namespace GameStore.Application.Features.Auth.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
