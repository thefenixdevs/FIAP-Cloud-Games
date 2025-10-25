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

    [HttpGet]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = _translator.Translate("Users.GetById.UserNotFound") });
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.NameIsRequired") });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.EmailIsRequired") });
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.UsernameIsRequired") });
        }

        var (success, message, user) = await _userService.CreateUserAsync(request);

        if (!success || user == null)
        {
            return BadRequest(new { message });
        }

        return Ok(new { message, user.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.NameIsRequired") });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.EmailIsRequired") });
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = _translator.Translate("Users.CreateUpdateUser.UsernameIsRequired") });
        }

        var (success, message) = await _userService.UpdateUserAsync(id, request);

        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(message);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ConfirmedAdmin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var (success, message) = await _userService.DeleteUserAsync(id);

        if (!success)
        {
            return NotFound(new { message });
        }

        return Ok(message);
    }
}
