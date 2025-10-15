using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Draw.it.Server.Extensions;
using Draw.it.Server.Controllers.Auth.DTO;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Auth;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Creates a new user and sets claims in cookie
    /// * For now just creates a new user every time
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(typeof(AuthMeResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Join([FromBody] AuthJoinRequestDto request)
    {
        // For simplicity, we create a new user every time. It's ok, since we don't store user data permanently.
        var user = _userService.CreateUser(request.Name);

        // Create identity with userId as claim
        var claims = new List<Claim>
        {
            // new Claim("userId", user.Id.ToString()) 
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())

        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Created("api/v1/auth/me", new AuthMeResponseDto(user));
    }

    /// <summary>
    /// Returns current user info
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthMeResponseDto), StatusCodes.Status200OK)]
    [Authorize]
    public IActionResult Me()
    {
        var user = HttpContext.ResolveUser(_userService);

        return Ok(new AuthMeResponseDto(user));
    }

    /// <summary>
    /// Logs out and clears cookie
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.ResolveUserId();

        _userService.DeleteUser(userId); // Clean up user, since it's anyway impossible to log back in
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedAccess() => Unauthorized("Not authenticated");
}
