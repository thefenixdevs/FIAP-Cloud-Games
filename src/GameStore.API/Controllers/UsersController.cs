using GameStore.API.Models.Responses;
using GameStore.Application.Features.Users.Shared;
using GameStore.Application.Features.Users.UseCases.CreateUser;
using GameStore.Application.Features.Users.UseCases.DeleteUser;
using GameStore.Application.Features.Users.UseCases.GetAllUsers;
using GameStore.Application.Features.Users.UseCases.GetUserById;
using GameStore.Application.Features.Users.UseCases.UpdateUser;
using GameStore.CrossCutting;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace GameStore.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de gerenciamento de usuários.
/// Requer autenticação JWT e permissões de administrador para todas as operações.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Users")]
[Produces("application/json")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IMediator mediator, 
        ILogger<UsersController> logger,
        IStringLocalizer<SharedResource> localizer) : base(localizer)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém a lista de todos os usuários cadastrados no sistema.
    /// </summary>
    /// <returns>Lista de usuários cadastrados com informações de gerenciamento.</returns>
    /// <response code="200">Lista de usuários retornada com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    [HttpGet]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(IEnumerable<UserManagementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserManagementResponse>>> GetUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await _mediator.Send(query);
        
        return ToActionResult(result);
    }

    /// <summary>
    /// Obtém um usuário específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID único do usuário (GUID).</param>
    /// <returns>Dados do usuário solicitado com informações de gerenciamento.</returns>
    /// <response code="200">Usuário encontrado e retornado com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    /// <response code="404">Usuário não encontrado com o ID fornecido.</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(UserManagementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserManagementResponse>> GetUser(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        return ToActionResultWithNotFound(result);
    }

    /// <summary>
    /// Cria um novo usuário no sistema.
    /// </summary>
    /// <param name="request">Dados do usuário a ser criado (nome, email, username, senha e tipo de perfil).</param>
    /// <returns>Dados do usuário criado incluindo o ID gerado.</returns>
    /// <response code="201">Usuário criado com sucesso. Retorna os dados do usuário criado.</response>
    /// <response code="400">Erro de validação dos dados fornecidos. Retorna detalhes dos campos inválidos.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(UserManagementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserManagementResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Name, request.Email, request.Username, request.Password, request.ProfileType);
        var result = await _mediator.Send(command);

        return ToCreatedAtAction(result, nameof(GetUser), new { id = result.Data?.Id });
    }

    /// <summary>
    /// Atualiza os dados de um usuário existente.
    /// </summary>
    /// <param name="id">ID único do usuário a ser atualizado (GUID).</param>
    /// <param name="request">Novos dados do usuário (nome, email, username, tipo de perfil e status da conta).</param>
    /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
    /// <response code="204">Usuário atualizado com sucesso.</response>
    /// <response code="400">Erro de validação dos dados fornecidos. Retorna detalhes dos campos inválidos.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    /// <response code="404">Usuário não encontrado com o ID fornecido.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(id, request.Name, request.Email, request.Username, request.ProfileType, request.AccountStatus);
        var result = await _mediator.Send(command);

        return ToNoContent(result);
    }

    /// <summary>
    /// Remove um usuário do sistema.
    /// </summary>
    /// <param name="id">ID único do usuário a ser removido (GUID).</param>
    /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
    /// <response code="204">Usuário removido com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    /// <response code="404">Usuário não encontrado com o ID fornecido.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);

        return ToNoContent(result);
    }
}
