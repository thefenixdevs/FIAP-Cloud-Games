using GameStore.Application.Features.Auth.DTOs;
using GameStore.Application.Features.Common;
using GameStore.Application.Features.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameStore.Domain.Exceptions;
using GameStore.API.Middleware;


namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
  IAuthService authService,
  ILogger<AuthController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;


    /// <summary>
    /// Realiza login de usuário.
    /// </summary>
    /// <remarks>Autentica por email ou username e retorna o token JWT.</remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Tentativa de login para o identificador: {Identifier}", request.Identifier);
       
        return Ok(await _authService.LoginAsync(request));
    }

    /// <summary>
    /// Confirma o email do usuário.
    /// </summary>
    /// <remarks>Utiliza userId e token enviado por email.</remarks>
    [HttpGet("confirm-email")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        _logger.LogInformation("Tentativa de confirmação de email para usuário {UserId} com token: {Token}", userId, token);
        await _authService.ConfirmEmailAsync(userId, token);

        return Ok(new MensagemResponse("Email confirmado com sucesso"));
    }

    /// <summary>
    /// Solicita reset de senha.
    /// </summary>
    /// <remarks>Envia e-mail com link para redefinir senha (expira em 5 minutos).</remarks>
    [HttpPost("request-password-reset")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetRequest request)
    {
        _logger.LogInformation("Solicitação de reset de senha para e-mail: {Email}", request.Email);
        await _authService.RequestPasswordResetAsync(request);
        return Ok(new MensagemResponse("Se o e-mail estiver cadastrado, você receberá instruções para redefinir sua senha"));
    }

    /// <summary>
    /// Confirma reset de senha.
    /// </summary>
    /// <remarks>Redefine a senha usando token enviado por e-mail.</remarks>
    [HttpPost("confirm-password-reset")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPasswordReset([FromQuery] Guid userId, [FromBody] ConfirmPasswordResetRequest request)
    {
        _logger.LogInformation("Confirmação de reset de senha para usuário {UserId}", userId);
        await _authService.ConfirmPasswordResetAsync(userId, request);
        return Ok(new MensagemResponse("Senha redefinida com sucesso"));
    }

    /// <summary>
    /// Solicita troca de e-mail.
    /// </summary>
    /// <remarks>Envia e-mail de confirmação para o e-mail atual antes de efetivar a troca.</remarks>
    [HttpPost("request-email-change")]
    [Authorize]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeRequest request)
    {
        _logger.LogInformation("Solicitação de troca de e-mail para: {NewEmail}", request.NewEmail);
        // TODO: Extrair userId do token JWT
        var userId = Guid.NewGuid(); // Placeholder - implementar extração do token
        await _authService.RequestEmailChangeAsync(userId, request);
        return Ok(new MensagemResponse("E-mail de confirmação enviado para seu e-mail atual"));
    }

    /// <summary>
    /// Confirma troca de e-mail.
    /// </summary>
    /// <remarks>Efetiva a troca de e-mail usando token enviado por e-mail.</remarks>
    [HttpGet("confirm-email-change")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailChange([FromQuery] Guid userId, [FromQuery] string token)
    {
        _logger.LogInformation("Confirmação de troca de e-mail para usuário {UserId}", userId);
        await _authService.ConfirmEmailChangeAsync(userId, token);
        return Ok(new MensagemResponse("E-mail alterado com sucesso"));
    }
}
