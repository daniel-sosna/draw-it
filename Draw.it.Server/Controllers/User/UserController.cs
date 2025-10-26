using System.Security.Claims;
using Draw.it.Server.Controllers.User.DTO;
using Draw.it.Server.Extensions;
using Draw.it.Server.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

    // Additional user-related endpoints can be added here
    // For example, fetching user details, updating user info, customizing user settings, etc.

    /// <summary>
    /// Updates username and reissues the authentication cookie to refresh session.
    /// </summary>
    [HttpPost("new-name")]
    public IActionResult UpdateName([FromBody] UpdateNameRequestDto request)
    {
        var userId = HttpContext.ResolveUserId();

        _userService.UpdateName(userId, request.name);

        return NoContent();
    }
}
