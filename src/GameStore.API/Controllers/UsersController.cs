using GameStore.Application.DTOs;
using GameStore.Application.Services;
using GameStore.CrossCutting.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

    [HttpGet("me")]
    [Authorize(Policy = "ConfirmedCommonUser")]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token" });
        }

        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { success = false, message = "User not found" });
        }

        return Ok(new
        {
            success = true,
            user = new UserResponse(
                user.Id,
                user.Username,
                user.Email,
                user.ProfileType,
                user.AccountStatus,
                user.CreatedAt
            )
        });
    }

}
