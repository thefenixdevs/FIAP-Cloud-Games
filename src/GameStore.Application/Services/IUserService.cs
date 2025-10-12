using GameStore.Application.DTOs;

namespace GameStore.Application.Services;

public interface IUserService
{
    Task<IEnumerable<UserManagementResponse>> GetAllUsersAsync();
    Task<UserManagementResponse?> GetUserByIdAsync(Guid id);
    Task<(bool Success, string Message, UserManagementResponse? User)> CreateUserAsync(CreateUserRequest request);
    Task<(bool Success, string Message)> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<(bool Success, string Message)> DeleteUserAsync(Guid id);
}
