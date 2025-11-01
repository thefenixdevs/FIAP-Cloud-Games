using GameStore.API.Models.Responses;
using GameStore.Application.Features.Auth.UseCases.Login;
using GameStore.Application.Features.Auth.UseCases.RegisterUser;
using GameStore.Application.Features.Auth.UseCases.SendAccountConfirmation;
using GameStore.Application.Features.Auth.UseCases.ValidationAccount;
using GameStore.CrossCutting;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace GameStore.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de autenticação e registro de usuários.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthController : BaseController
{
  private readonly IMediator _mediator;
  private readonly ILogger<AuthController> _logger;

  public AuthController(
    IMediator mediator, 
    ILogger<AuthController> logger,
    IStringLocalizer<SharedResource> localizer) : base(localizer)
  {
    _mediator = mediator;
    _logger = logger;
  }

  /// <summary>
  /// Registra um novo usuário no sistema.
  /// </summary>
  /// <param name="request">Dados do usuário para registro (nome, email, username e senha).</param>
  /// <returns>ID do usuário criado em caso de sucesso, ou erros de validação em caso de falha.</returns>
  /// <response code="200">Registro realizado com sucesso. Retorna o ID do usuário criado.</response>
  /// <response code="400">Erro de validação dos dados fornecidos. Retorna detalhes dos campos inválidos.</response>
  [HttpPost("register")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<Guid?>> Register([FromBody] RegisterUserRequest request)
  {
    _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

    var command = new RegisterUserCommand(request.Name, request.Email, request.Username, request.Password);
    var response = await _mediator.Send(command);
    
    if (!response.IsSuccess)
    {
      return ToActionResult(response);
    }

    return Ok(new { message = TranslatedMessage(response.Message), userId = response.Data });
  }

  /// <summary>
  /// Realiza login de um usuário no sistema.
  /// </summary>
  /// <param name="request">Credenciais de login (identifier pode ser email ou username, e senha).</param>
  /// <returns>Dados do usuário autenticado com token JWT em caso de sucesso, ou erro em caso de falha.</returns>
  /// <response code="200">Login realizado com sucesso. Retorna os dados do usuário e o token JWT.</response>
  /// <response code="401">Credenciais inválidas ou usuário não encontrado.</response>
  [HttpPost("login")]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    _logger.LogInformation("Login attempt for identifier: {Identifier}", request.Identifier);

    var command = new LoginCommand(request.Identifier, request.Password);
    var result = await _mediator.Send(command);

    if (!result.IsSuccess || result.Data == null)
    {
      // Para login, erros devem retornar Unauthorized ao invés de BadRequest
      return Unauthorized(new 
      { 
        message = TranslatedMessage(result.Message), 
        errors = FormatErrors(result)
      });
    }

    return Ok(new { response = result.Data, success = result.IsSuccess });
  }

  /// <summary>
  /// Envia código de confirmação por email para validar a conta do usuário.
  /// </summary>
  /// <param name="request">Email do usuário que receberá o código de confirmação.</param>
  /// <returns>Mensagem de sucesso ou erro.</returns>
  /// <response code="200">Código de confirmação enviado com sucesso.</response>
  /// <response code="400">Erro ao enviar código de confirmação (usuário não encontrado ou email inválido).</response>
  [HttpPost("sendConfirmation")]
  [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SendConfirmation([FromBody] SendAccountConfirmationRequest request)
  {
    _logger.LogInformation("Confirmation send for email: {Email}", request.Email);

    var command = new SendAccountConfirmationCommand(request.Email);
    var result = await _mediator.Send(command);

    return ToActionResult(result);
  }

  /// <summary>
  /// Valida a conta do usuário usando o código de confirmação enviado por email.
  /// </summary>
  /// <param name="Code">Código de confirmação recebido por email (codificado em Base64).</param>
  /// <returns>Mensagem de sucesso ou erro.</returns>
  /// <response code="200">Conta validada com sucesso.</response>
  /// <response code="400">Código inválido, expirado ou conta já validada.</response>
  [HttpGet("validationAccount")]
  [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ValidationAccount([FromQuery] string Code)
  {
    _logger.LogInformation("Code for attempt validation: {Code}", Code);

    var command = new ValidationAccountCommand(Code);
    var result = await _mediator.Send(command);

    return ToActionResult(result);
  }
}
