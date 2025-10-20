using GameStore.Application.Features.Games.DTOs;
using GameStore.Application.Features.Games.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IGameService gameService,
        ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "ConfirmedCommonUser")]
    /// <summary>
    /// Lista jogos.
    /// </summary>
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GameResponse>>> GetGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedCommonUser")]
    /// <summary>
    /// Obtém jogo por Id.
    /// </summary>
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameResponse>> GetGame(Guid id)
    {
        var game = await _gameService.GetGameByIdAsync(id);

        if (game == null)
        {
            return NotFound(new { message = "Game not found" });
        }

        return Ok(game);
    }

    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    /// <summary>
    /// Cria jogo (apenas admin).
    /// </summary>
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameResponse>> CreateGame([FromBody] CreateGameRequest request)
    {
        var (success, message, game) = await _gameService.CreateGameAsync(request);

        if (!success || game == null)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    /// <summary>
    /// Atualiza jogo (apenas admin).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGame(Guid id, [FromBody] UpdateGameRequest request)
    {
        var (success, message) = await _gameService.UpdateGameAsync(id, request);

        if (!success)
        {
            return NotFound(new { message });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    /// <summary>
    /// Exclui jogo (apenas admin).
    /// </summary>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGame(Guid id)
    {
        var (success, message) = await _gameService.DeleteGameAsync(id);

        if (!success)
        {
            return NotFound(new { message });
        }

        return NoContent();
    }
}
