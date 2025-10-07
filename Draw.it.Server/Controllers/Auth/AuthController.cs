using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Draw.it.Server.Controllers.Auth.DTO;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Auth;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{

    public AuthController(IUserService userService)
        : base(userService)
    {
    }

    /// <summary>
    /// Creates a new user and sets claims in cookie
    /// * For now just creates a new user every time
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Join([FromBody] AuthJoinRequestDto request)
    {
        // For simplicity, we create a new user every time. It's ok, since we don't store user data permanently.
        var user = _userService.CreateUser(request.Name);

        // Create identity with userId as claim
        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Created();
    }

    /// <summary>
    /// Returns current user info
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthMeResponseDto), StatusCodes.Status200OK)]
    [Authorize]
    public IActionResult Me()
    {
        var user = ResolveUser();

        return Ok(new AuthMeResponseDto(user));
    }

    /// <summary>
    /// Logs out and clears cookie
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = ResolveUserId();

        _userService.DeleteUser(userId); // Clean up user, since it's anyway impossible to log back in
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedAccess() => Unauthorized("Not authenticated");
}
