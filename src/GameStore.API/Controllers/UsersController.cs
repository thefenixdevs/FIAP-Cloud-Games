using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.CrossCutting.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly ITranslationService _translator;

    public UsersController(IUserService userService, ILogger<UsersController> logger, ITranslationService translator)
    {
        _userService = userService;
        _logger = logger;
        _translator = translator;
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = _translator.Translate("UserNotFound") });
        }

        return Ok(user);
    }

}
