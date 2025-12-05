using GameStore.Application.DTOs;
using GameStore.Domain.Entities;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserManagementResponse?> GetUserByIdAsync(Guid id)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user is null)
        {
            _logger.LogWarning("User with ID {UserId} not found", id);
            return null;
        }

        return MapToResponse(user);
    }
    private static UserManagementResponse MapToResponse(User user)
        => new(
            user.Id,
            user.Name,
            user.Username,
            user.Email,
            user.ProfileType,
            user.AccountStatus,
            user.CreatedAt
        );
}