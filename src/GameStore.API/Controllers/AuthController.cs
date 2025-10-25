using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.CrossCutting.Localization;
using Microsoft.AspNetCore.Mvc;


namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  private readonly ILogger<AuthController> _logger;
  private readonly IEncriptService _encriptService;
  private readonly ITranslationService _translator;

  public AuthController(IAuthService authService, ILogger<AuthController> logger, IEncriptService encriptService, ITranslationService translator)
  {
    _authService = authService;
    _logger = logger;
    _encriptService = encriptService;
    _translator = translator;
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
      return BadRequest(new { message = _translator.Translate("Auth.Register.AllfieldsAreRequired") });
    }

    if (request.Password.Length < 8)
    {
      return BadRequest(new { message = _translator.Translate("Auth.Register.PasswordMustBeAtLeast8CharactersLong") });
    }

    var (success, message, userId) = await _authService.RegisterAsync(request);
    string translatedMessage = _translator.Translate(message);
    
    if (!success)
    {
      return BadRequest(new { message = translatedMessage });
    }

    return Ok(new { message = translatedMessage, userId });
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    _logger.LogInformation("Login attempt for identifier: {Identifier}", request.Identifier);

    if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
    {
      return BadRequest(new { message = _translator.Translate("Auth.Login.IdentifierAndPasswordAreRequired") });
    }

    var (success, message, response) = await _authService.LoginAsync(request);
    string translatedMessage = _translator.Translate(message);

    if (!success || response == null)
    {
      return Unauthorized(new { message = translatedMessage });
    }

    return Ok(new { message });
  }

  [HttpPost("sendConfirmation")]
  public async Task<IActionResult> SendConfirmation([FromBody] ValidationNotificationRequest request)
  {
    _logger.LogInformation("Confirmation send for email: {Email}", request.Email);

    if (string.IsNullOrWhiteSpace(request.Email))
        return BadRequest(new { message = "E-mail are required" });

    var (success, message, response) = await _authService.SendAccountConfirmationAsync(request);
    if (!success)
        return BadRequest(new { message });

    return Ok(new { message });
  }

  [HttpGet("validationAccount")]
  public async Task<IActionResult> ValidationAccount([FromQuery] string Code)
  {
    _logger.LogInformation("Code for attempt validation: {Code}", Code);

    var decoded = _encriptService.DecodeMaskedCode(Code);
    if (decoded == null)
        return BadRequest(new { message = "Invalid code" });

    _logger.LogInformation("Email validation attempt for: {Email}", decoded.Value.Email);

    var request = new ValidationAccountRequest(decoded.Value.Email, DateTime.Parse(decoded.Value.Expiration));

    if (string.IsNullOrWhiteSpace(request.Email))
        return BadRequest(new { message = "E-mail are required" });

    var (success, message, response) = await _authService.ValidationAccountAsync(request);

    if (!success)
        return BadRequest(new { message });

    return Ok(new { message });
  }
}
