using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.Session;
using Draw.it.Server.Services.User;
using Draw.it.Server.Models.Session;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Controllers.Room;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;

    public RoomController(IRoomService roomService, ISessionService sessionService, IUserService userService)
    {
        _roomService = roomService;
        _sessionService = sessionService;
        _userService = userService;
    }

    // Helper to get current user and session from claims
    private (UserModel user, SessionModel session) ResolveUserSession()
    {
        var sessionId = (User.FindFirst("sessionId")?.Value) ?? throw new UnauthorizedUserException("Session ID claim missing.");

        var session = _sessionService.GetSession(sessionId);
        var user = _userService.GetUser(session.UserId);
        return (user, session);
    }

    [HttpPost("create")]
    public IActionResult CreateRoom()
    {
        var (user, session) = ResolveUserSession();
        var room = _roomService.CreateRoom(user.Id);

        _sessionService.SetRoom(session.Id, room.Id);

        return Created($"/api/v1/room/{room.Id}", new { room.Id, host = user });
    }

    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();
        if (session.RoomId != null)
            return BadRequest("User is already in a room.");

        _roomService.JoinRoom(roomId, user.Id);
        session.RoomId = roomId;

        return NoContent();
    }

    [HttpPost("{roomId}/leave")]
    public IActionResult LeaveRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();
        if (session.RoomId == null)
            return BadRequest("User is not in a room.");
        if (session.RoomId != roomId)
            return BadRequest("User is not in this room.");

        _roomService.LeaveRoom(roomId, user.Id);
        session.RoomId = null;

        return NoContent();
    }

    [HttpGet("{roomId}")]
    [AllowAnonymous]
    public IActionResult GetRoom(string roomId)
    {
        var room = _roomService.GetRoom(roomId);

        return Ok(room);
    }

    [HttpDelete("{roomId}")]
    public IActionResult DeleteRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();
        var room = _roomService.GetRoom(roomId);
        if (room.HostId != user.Id)
            return Forbid();

        _roomService.DeleteRoom(roomId);
        if (session.RoomId == roomId)
            session.RoomId = null;

        return NoContent();
    }
}
