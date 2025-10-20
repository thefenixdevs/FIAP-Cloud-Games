using GameStore.Application.Features.Users.DTOs;

namespace GameStore.Application.Features.Users.Interfaces;

public interface IUserService
{
    Task RegisterAsync(RegisterRequest request);
    Task<IEnumerable<UserManagementResponse>> GetAllUsersAsync();
    Task<UserManagementResponse?> GetUserByIdAsync(Guid id);
    Task<UserManagementResponse> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeleteUserAsync(Guid id);
}
