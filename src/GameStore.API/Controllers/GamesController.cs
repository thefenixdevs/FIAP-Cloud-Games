using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.CrossCutting.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;
    private readonly ITranslationService _translator;

    public GamesController(IGameService gameService, ILogger<GamesController> logger, ITranslationService translator)
    {
        _gameService = gameService;
        _logger = logger;
        _translator = translator;
    }

    [HttpGet]
    [Authorize(Policy = "ConfirmedCommonUser")]
    public async Task<ActionResult<IEnumerable<GameResponse>>> GetGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedCommonUser")]
    public async Task<ActionResult<GameResponse>> GetGame(Guid id)
    {
        var game = await _gameService.GetGameByIdAsync(id);

        if (game == null)
        {
            return NotFound(new { message = _translator.Translate("GameNotFound") });
        }

        return Ok(game);
    }

    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<ActionResult<GameResponse>> CreateGame([FromBody] CreateGameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = _translator.Translate("Games.CreateUpdateGame.TitleIsRequired") });
        }

        if (request.Price < 0)
        {
            return BadRequest(new { message = _translator.Translate("Game.CreateUpdateGame.PriceCannotBeNegative") });
        }

        var (success, message, game) = await _gameService.CreateGameAsync(request);
        string translatedMessage = _translator.Translate(message);

        if (!success || game == null)
        {
            return BadRequest(new { message = translatedMessage });
        }

        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateGame(Guid id, [FromBody] UpdateGameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = _translator.Translate("Games.CreateUpdateGame.TitleIsRequired") });
        }

        if (request.Price < 0)
        {
            return BadRequest(new { message = _translator.Translate("Game.CreateUpdateGame.PriceCannotBeNegative") });
        }

        var (success, message) = await _gameService.UpdateGameAsync(id, request);
        string translatedMessage = _translator.Translate(message);

        if (!success)
        {
            return NotFound(new { message = translatedMessage });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteGame(Guid id)
    {
        var (success, message) = await _gameService.DeleteGameAsync(id);
        string translatedMessage = _translator.Translate(message);

        if (!success)
        {
            return NotFound(new { message = translatedMessage });
        }

        return NoContent();
    }
}
