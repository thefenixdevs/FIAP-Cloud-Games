using GameStore.Application.DTOs;
using GameStore.Application.Services;
using Microsoft.AspNetCore.Mvc;


namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly ILogger<AuthController> _logger;

  public AuthController(IAuthService authService, ILogger<AuthController> logger)
  {
    _authService = authService;
    _logger = logger;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequest request)
  {
    _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

    if (string.IsNullOrWhiteSpace(request.Name) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
      return BadRequest(new { message = "All fields are required" });
    }

    if (request.Password.Length < 8)
    {
      return BadRequest(new { message = "Password must be at least 8 characters long" });
    }

    var (success, message, userId) = await _authService.RegisterAsync(request);

    if (!success)
    {
      return BadRequest(new { message });
    }

    return Ok(new { message, userId });
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    _logger.LogInformation("Login attempt for identifier: {Identifier}", request.Identifier);

    if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
    {
      return BadRequest(new { message = "Identifier and password are required" });
    }

    var (success, message, response) = await _authService.LoginAsync(request);

    if (!success || response == null)
    {
      return Unauthorized(new { message });
    }

    return Ok(response);
  }
}
