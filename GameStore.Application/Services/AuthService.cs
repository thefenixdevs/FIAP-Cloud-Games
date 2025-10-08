using GameStore.Application.DTOs;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, Guid? UserId)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                return (false, "Email already exists", null);
            }

            if (await _unitOfWork.Users.ExistsByUsernameAsync(request.Username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
                return (false, "Username already exists", null);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User(request.Email, request.Username, passwordHash);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {Username} registered successfully with ID {UserId}", user.Username, user.Id);
            return (true, "User registered successfully", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
            return (false, "An error occurred during registration", null);
        }
    }

    public async Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
                return (false, "Invalid email or password", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);
                return (false, "Invalid email or password", null);
            }

            if (user.AccountStatus == AccountStatus.Banned)
            {
                _logger.LogWarning("Login failed: Account is banned for email {Email}", request.Email);
                return (false, "Account is banned", null);
            }

            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponse(
                user.Id,
                user.Username,
                user.Email,
                token,
                user.ProfileType,
                user.AccountStatus);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);
            return (true, "Login successful", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login for email {Email}", request.Email);
            return (false, "An error occurred during login", null);
        }
    }
}
