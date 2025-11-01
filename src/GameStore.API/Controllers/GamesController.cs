using GameStore.API.Models.Responses;
using GameStore.Application.Features.Games.Shared;
using GameStore.Application.Features.Games.UseCases.CreateGame;
using GameStore.Application.Features.Games.UseCases.DeleteGame;
using GameStore.Application.Features.Games.UseCases.GetAllGames;
using GameStore.Application.Features.Games.UseCases.GetGameById;
using GameStore.Application.Features.Games.UseCases.UpdateGame;
using GameStore.CrossCutting;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace GameStore.API.Controllers;

/// <summary>
/// Controller responsável pelos endpoints de gerenciamento de jogos.
/// Requer autenticação JWT e diferentes níveis de permissão conforme a operação.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Games")]
[Produces("application/json")]
public class GamesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IMediator mediator, 
        ILogger<GamesController> logger,
        IStringLocalizer<SharedResource> localizer) : base(localizer)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém a lista de todos os jogos cadastrados no sistema.
    /// </summary>
    /// <returns>Lista de jogos cadastrados.</returns>
    /// <response code="200">Lista de jogos retornada com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não confirmado ou sem permissão suficiente.</response>
    [HttpGet]
    [Authorize(Policy = "ConfirmedCommonUser")]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<GameResponse>>> GetGames()
    {
        var query = new GetAllGamesQuery();
        var result = await _mediator.Send(query);
        
        return ToActionResult(result);
    }

    /// <summary>
    /// Obtém um jogo específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID único do jogo (GUID).</param>
    /// <returns>Dados do jogo solicitado.</returns>
    /// <response code="200">Jogo encontrado e retornado com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não confirmado ou sem permissão suficiente.</response>
    /// <response code="404">Jogo não encontrado com o ID fornecido.</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedCommonUser")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameResponse>> GetGame(Guid id)
    {
        var query = new GetGameByIdQuery(id);
        var result = await _mediator.Send(query);

        return ToActionResultWithNotFound(result);
    }

    /// <summary>
    /// Cria um novo jogo no sistema.
    /// </summary>
    /// <param name="request">Dados do jogo a ser criado (título, descrição, preço, gênero e data de lançamento).</param>
    /// <returns>Dados do jogo criado incluindo o ID gerado.</returns>
    /// <response code="201">Jogo criado com sucesso. Retorna os dados do jogo criado.</response>
    /// <response code="400">Erro de validação dos dados fornecidos. Retorna detalhes dos campos inválidos.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GameResponse>> CreateGame([FromBody] CreateGameRequest request)
    {
        var command = new CreateGameCommand(request.Title, request.Description, request.Price, request.Genre, request.ReleaseDate);
        var result = await _mediator.Send(command);

        return ToCreatedAtAction(result, nameof(GetGame), new { id = result.Data?.Id });
    }

    /// <summary>
    /// Atualiza os dados de um jogo existente.
    /// </summary>
    /// <param name="id">ID único do jogo a ser atualizado (GUID).</param>
    /// <param name="request">Novos dados do jogo (título, descrição, preço, gênero e data de lançamento).</param>
    /// <returns>Mensagem de sucesso em caso de atualização bem-sucedida.</returns>
    /// <response code="200">Jogo atualizado com sucesso. Retorna mensagem de confirmação.</response>
    /// <response code="400">Erro de validação dos dados fornecidos. Retorna detalhes dos campos inválidos.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    /// <response code="404">Jogo não encontrado com o ID fornecido.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGame(Guid id, [FromBody] UpdateGameRequest request)
    {
        var command = new UpdateGameCommand(id, request.Title, request.Description, request.Price, request.Genre, request.ReleaseDate);
        var result = await _mediator.Send(command);

        return ToNoContent(result);
    }

    /// <summary>
    /// Remove um jogo do sistema.
    /// </summary>
    /// <param name="id">ID único do jogo a ser removido (GUID).</param>
    /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
    /// <response code="204">Jogo removido com sucesso.</response>
    /// <response code="401">Token de autenticação não fornecido ou inválido.</response>
    /// <response code="403">Usuário não é administrador ou conta não confirmada.</response>
    /// <response code="404">Jogo não encontrado com o ID fornecido.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame(Guid id)
    {
        var command = new DeleteGameCommand(id);
        var result = await _mediator.Send(command);

        return ToNoContent(result);
    }
}
