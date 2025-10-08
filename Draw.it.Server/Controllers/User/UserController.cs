using Draw.it.Server.Controllers.User.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.User;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Set player ready status.
    /// </summary>
    [HttpPut("{userId}/ready")]
    public IActionResult SetUserReadyStatus([FromRoute] long userId, [FromBody] SetReadyStatusRequestDto request)
    {
        _userService.SetReady(userId, request.IsReady);

        return NoContent();
    }

    // Additional user-related endpoints can be added here
    // For example, fetching user details, updating user info, customizing user settings, etc.
}
