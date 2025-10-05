using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Draw.it.Server.Controllers.Session.DTO;
using Draw.it.Server.Services.Session;

namespace Draw.it.Server.Controllers.Session;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public AuthController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinRequest request)
    {
        var session = _sessionService.CreateSession(request.UserId, request.RoomId);

        // Create identity with sessionId as claim
        var claims = new List<Claim>
        {
            new Claim("SessionId", session.Id),
            new Claim("UserId", session.UserId.ToString()),
            new Claim("RoomId", session.RoomId ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Ok(new { message = "Joined successfully", sessionId = session.Id });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var sessionId = User.FindFirst("SessionId")?.Value;

        if (sessionId == null) return Unauthorized();

        var session = _sessionService.GetSession(sessionId);

        return session is not null
            ? Ok(session)
            : Unauthorized();
    }
}
