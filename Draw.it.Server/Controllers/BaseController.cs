using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Session;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Session;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly ISessionService _sessionService;
    protected readonly IUserService _userService;

    protected BaseController(ISessionService sessionService, IUserService userService)
    {
        _sessionService = sessionService;
        _userService = userService;
    }

    // Helper to get current session from claims
    protected SessionModel ResolveSession()
    {
        var sessionId = (User.FindFirst("sessionId")?.Value) ?? throw new UnauthorizedUserException("Session ID claim missing.");

        return _sessionService.GetSession(sessionId);
    }

    // Helper to get current user and session from claims
    protected (UserModel user, SessionModel session) ResolveUserAndSession()
    {
        var session = ResolveSession();
        var user = _userService.GetUser(session.UserId);

        return (user, session);
    }

    // Helper to get current user from claims
    protected UserModel ResolveUser()
    {
        var (user, _) = ResolveUserAndSession();
        return user;
    }
}
