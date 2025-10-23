using GameStore.Application.DTOs;
using GameStore.Domain.Entities;
using GameStore.Domain.Enums;
using GameStore.Domain.Repositories.Abstractions;
using GameStore.Domain.Security;
using GameStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Services;

public class AuthService : IAuthService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;
  private readonly ILogger<AuthService> _logger;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IEmailService _emailService;
  private readonly IEncriptService _encriptService;

  public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<AuthService> logger, IPasswordHasher passwordHasher, IEmailService emailService, IEncriptService encriptService)
  {
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
    _logger = logger;
    _passwordHasher = passwordHasher;
    _emailService = emailService;
    _encriptService = encriptService;
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

      var user = User.Register(request.Name, request.Email, request.Username, request.Password, _passwordHasher);

      await _unitOfWork.Users.AddAsync(user);
      await _unitOfWork.CommitAsync();

      var masked = _encriptService.EncodeMaskedCode(request.Email);
      var confirmationLink = $"https://localhost:7055/api/Auth/ValidationAccount?code={masked}";
      var htmlBody = _emailService.TemplateEmailConfirmation(confirmationLink);

      await _emailService.SendConfirmationEmailAsync(
          request.Email,
          "Confirmação de conta",
          htmlBody
      );

      _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
      _logger.LogInformation("User {Username} registered successfully with ID {UserId}", user.Username, user.Id);
      return (true, "User registered successfully", user.Id);
    }
    catch (ArgumentException ex) when (ex.ParamName == "password")
    {
      _logger.LogWarning("Registration failed: Invalid password requirements for email {Email}", request.Email);
      return (false, ex.Message, null);
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
      var identifier = request.Identifier.Trim();
      User? user = null;
      var lookedUpBy = "identifier";

      try
      {
        var email = Email.Create(identifier);
        user = await _unitOfWork.Users.GetByEmailAsync(email.Value);
        lookedUpBy = "email";
      }
      catch (ArgumentException)
      {
        user = await _unitOfWork.Users.GetByUsernameAsync(identifier);
        lookedUpBy = "username";
      }

      if (user == null)
      {
        _logger.LogWarning("Login failed: User with identifier {Identifier} not found", request.Identifier);
        return (false, "Invalid credentials", null);
      }

      if (!user.VerifyPassword(request.Password, _passwordHasher))
      {
        _logger.LogWarning("Login failed: Invalid password for identifier {Identifier}", request.Identifier);
        return (false, "Invalid credentials", null);
      }

      switch (user.AccountStatus)
      {
        case AccountStatus.Pending:
          _logger.LogWarning("Login failed: Account pending confirmation for identifier {Identifier}", request.Identifier);
          return (false, "Account pending email confirmation", null);
        case AccountStatus.Blocked:
          _logger.LogWarning("Login failed: Account is blocked for identifier {Identifier}", request.Identifier);
          return (false, "Account is blocked", null);
        case AccountStatus.Banned:
          _logger.LogWarning("Login failed: Account is banned for identifier {Identifier}", request.Identifier);
          return (false, "Account is banned", null);
      }

      var token = _jwtService.GenerateToken(user);

      var response = new LoginResponse(
        user.Id,
        user.Username,
        user.Email.Value,
        token,
        user.ProfileType,
        user.AccountStatus);

      _logger.LogInformation("User {Username} logged in successfully using {Lookup}", user.Username, lookedUpBy);
      return (true, "Login successful", response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during user login for identifier {Identifier}", request.Identifier);
      return (false, "An error occurred during login", null);
    }
  }

  public async Task<(bool Success, string Message, ValidationNotificationResponse? Response)> SendAccountConfirmationAsync(ValidationNotificationRequest request)
  {
    User? user = null;

    if (request is null || string.IsNullOrWhiteSpace(request.Email))
      return (false, "E-mail is required", null);

    try
    {
      user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
      if (user == null)
        return (false, "User not found", null);

      var masked = _encriptService.EncodeMaskedCode(request.Email);
      var confirmationLink = $"https://localhost:7055/api/Auth/ValidationAccount?code={masked}";
      var htmlBody = _emailService.TemplateEmailConfirmation(confirmationLink);

      await _emailService.SendConfirmationEmailAsync(
          user.Email,
          "Confirmação de conta",
          htmlBody
      );

      _logger.LogInformation("Confirmation email sent to {Email}", user.Email);
      return (true, "Confirmation email sent successfully", null);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while sending confirmation e-mail for {Email}", request.Email);
      return (false, "Failed to send confirmation e-mail", null);
    }
  }

  public async Task<(bool Success, string Message, ValidationAccountResponse? Response)> ValidationAccountAsync(ValidationAccountRequest request)
  {
    User? user = null;

    if (request is null || string.IsNullOrWhiteSpace(request.Email))
      return (false, "E-mail is required", null);

    try
    {
      user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
      if (user == null)
        return (false, "User not found", null);

      if (user.AccountStatus == AccountStatus.Active)
        return (false, "Account already confirmed", null);

      if (DateTime.Now > request.Expiration.AddMinutes(15))
        return (false, "Activation link expired", null);

      user = User.Update(user, user.Name, user.Email, user.Username, AccountStatus.Active, user.ProfileType);

      await _unitOfWork.Users.UpdateAsync(user);
      await _unitOfWork.CommitAsync();

      _logger.LogInformation("Account {Email} successfully confirmed", user.Email);
      return (true, "Account confirmed successfully", null);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during account validation for {Email}", request.Email);
      return (false, "An error occurred during account validation", null);
    }
  }
}
