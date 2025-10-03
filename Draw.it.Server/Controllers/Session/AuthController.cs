using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Draw.it.Server.Models.Session;
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
        var session = new SessionModel
        {
            UserName = request.Name,
            RoomId = request.RoomId
        };

        _sessionService.Add(session);

        // Create identity with sessionId as claim
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, session.UserName),
            new Claim("SessionId", session.SessionId),
            new Claim("RoomId", session.RoomId)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Ok(new { message = "Joined successfully", sessionId = session.SessionId });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var sessionId = User.FindFirst("SessionId")?.Value;

        if (sessionId == null) return Unauthorized();

        var session = _sessionService.Get(sessionId);

        return session is not null
            ? Ok(session)
            : Unauthorized();
    }

    public class JoinRequest
    {
        public string Name { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
    }
}
