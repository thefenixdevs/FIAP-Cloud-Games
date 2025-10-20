using GameStore.Application.Features.Auth.DTOs;
using GameStore.Application.Features.Users.DTOs;
using GameStore.Application.Features.Users.Interfaces;
using GameStore.Application.Features.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Auto-registro de usuário.
    /// </summary>
    /// <remarks>Cria um novo usuário e envia email de confirmação.</remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Tentativa de auto-registro para o email: {Email}", request.Email);

        await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = Guid.Empty }, new MensagemResponse("Usuário registrado com sucesso. Verifique seu email para confirmar a conta."));
    }

    /// <summary>
    /// Lista usuários (apenas admin).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Obtém usuário por Id (apenas admin).
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Cria usuário (apenas admin).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(UserManagementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Atualiza usuário (apenas admin).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        await _userService.UpdateUserAsync(id, request);
        return Ok(new MensagemResponse("Usuário atualizado com sucesso"));
    }

    /// <summary>
    /// Exclui usuário (apenas admin).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(MensagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new MensagemResponse("Usuário excluído com sucesso"));
    }
}
