using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Draw.it.Server.Controllers.Session.DTO;
using Draw.it.Server.Services.Session;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Session;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{

    public AuthController(IUserService userService, ISessionService sessionService)
        : base(sessionService, userService)
    {
    }

    /// <summary>
    /// Creates a new session for a user and sets cookie
    /// * For now just creates a new user every time
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Join([FromBody] SessionJoinRequestDto request)
    {
        // For simplicity, we create a new user every time. It's ok, since we don't store user data permanently.
        var user = _userService.CreateUser(request.Name);
        var session = _sessionService.CreateSession(user.Id);

        // Create identity with sessionId as claim
        var claims = new List<Claim>
        {
            new Claim("sessionId", session.Id)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Created();
    }

    /// <summary>
    /// Returns current user & session info
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(SessionMeResponseDto), StatusCodes.Status200OK)]
    [Authorize]
    public IActionResult Me()
    {
        var user = ResolveUser();

        return Ok(new SessionMeResponseDto(user));
    }

    /// <summary>
    /// Logs out and clears cookie
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var sessionId = User.FindFirst("sessionId")?.Value;
        if (sessionId != null)
            _sessionService.DeleteSession(sessionId);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("unauthorized")]
    public IActionResult UnauthorizedAccess() => Unauthorized("Not authenticated");
}
