using GameStore.Application.DTOs;

namespace GameStore.Application.Services;

public interface IUserService
{
    Task<UserManagementResponse?> GetUserByIdAsync(Guid id);
}
