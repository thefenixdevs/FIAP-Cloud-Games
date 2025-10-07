using GameStore.Domain.Entities;

namespace GameStore.Application.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
