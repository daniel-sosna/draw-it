using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Controllers.Room.DTO;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.Session;
using Draw.it.Server.Services.User;
using Draw.it.Server.Models.Room;
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

        var roomId = _roomService.GenerateUniqueRoomId();
        _roomService.CreateRoom(roomId, new RoomSettingsModel());
        var room = _roomService.GetRoom(roomId);
        room.Players.Add(user);

        session.RoomId = roomId;
        return Created($"/api/v1/room/{roomId}", new { roomId, host = user });
    }

    [HttpPost("join")]
    public IActionResult JoinRoom([FromBody] JoinRoomRequest request)
    {
        var (user, session) = ResolveUserSession();
        var success = _roomService.JoinRoom(request.RoomId, user);
        if (!success) return BadRequest("Room not found or cannot join.");

        session.RoomId = request.RoomId;

        var room = _roomService.GetRoom(request.RoomId);
        return Ok(room);
    }

    [HttpPost("leave")]
    public IActionResult LeaveRoom()
    {
        var (user, session) = ResolveUserSession();
        if (session.RoomId == null) return BadRequest("User is not in a room.");

        var room = _roomService.GetRoom(session.RoomId);
        room.Players.RemoveAll(p => p.Id == user.Id);
        session.RoomId = null;

        return NoContent();
    }

    [HttpGet("{roomId}")]
    [AllowAnonymous]
    public IActionResult GetRoom(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        if (room == null) return NotFound();

        return Ok(room);
    }

    [HttpDelete("{roomId}")]
    public IActionResult DeleteRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();
        var room = _roomService.GetRoom(roomId);
        if (room == null) return NotFound();

        // Host verification could be implemented with room.HostId or similar
        if (room.Players.FirstOrDefault()?.Id != user.Id)
            return Forbid();

        _roomService.DeleteRoom(roomId);
        return NoContent();
    }
}
