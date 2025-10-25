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

  public async Task<IEnumerable<UserManagementResponse>> GetAllUsersAsync()
  {
    _logger.LogInformation("Fetching all users");

    var users = await _unitOfWork.Users.GetAllAsync();
    return users.Select(MapToResponse);
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

  public async Task<(bool Success, string Message, UserManagementResponse? User)> CreateUserAsync(CreateUserRequest request)
  {
    try
    {
      _logger.LogInformation("Creating new user: {Username}", request.Username);

      if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
      {
        _logger.LogWarning("Create User failed: Email {Email} already exists", request.Email);
        return (false, "EmailAlreadyExists", null);
      }

      if (await _unitOfWork.Users.ExistsByUsernameAsync(request.Username))
      {
        _logger.LogWarning("Create User failed: Username {Username} already exists", request.Username);
        return (false, "UsernameAlreadyExists", null);
      }

      var user = User.Register(request.Name, request.Email, request.Username, request.Password, _passwordHasher, request.ProfileType);

      await _unitOfWork.Users.AddAsync(user);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
      return (true, "UserRegisteredSuccessfully", MapToResponse(user));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating user: {Username}", request.Username);
      return (false, "UserService.CreateUserAsync.AnErrorOccurredWhileCreatingTheUser", null);
    }
  }

  public async Task<(bool Success, string Message)> UpdateUserAsync(Guid id, UpdateUserRequest request)
  {
    try
    {
      _logger.LogInformation("Updating user with ID: {UserId}", id);

      var user = await _unitOfWork.Users.GetByIdAsync(id);
      if (user is null)
      {
        _logger.LogWarning("User with ID {UserId} not found", id);
        return (false, "UserNotFound");
      }

      if(user.Email.Value != request.Email && await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
      {
        _logger.LogWarning("Create User failed: Email {Email} already exists", request.Email);
        return (false, "EmailAlreadyExists");
      }

      if (user.Username != request.Username && await _unitOfWork.Users.ExistsByUsernameAsync(request.Username))
      {
        _logger.LogWarning("Create User failed: Username {Username} already exists", request.Username);
        return (false, "UsernameAlreadyExists");
      }

      user = User.Update(user, request.Name, request.Email, request.Username, request.AccountStatus, request.ProfileType);

      await _unitOfWork.Users.UpdateAsync(user);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("User with ID {UserId} updated successfully", id);
      return (true, "UserUpdatedSuccessfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
      return (false, "UserService.UpdateUserAsync.AnErrorOccurredWhileUpdatingTheUser");
    }
  }

  public async Task<(bool Success, string Message)> DeleteUserAsync(Guid id)
  {
    try
    {
      _logger.LogInformation("Deleting user with ID: {UserId}", id);

      var user = await _unitOfWork.Users.GetByIdAsync(id);
      if (user is null)
      {
        _logger.LogWarning("User with ID {UserId} not found", id);
        return (false, "UserNotFound");
      }

      await _unitOfWork.Users.DeleteAsync(user.Id);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("User with ID {UserId} deleted successfully", id);
      return (true, "UserDeletedSuccessfully");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
      return (false, "UserService.DeleteUserAsync.AnErrorOccurredWhileDeletingTheUser");
    }
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