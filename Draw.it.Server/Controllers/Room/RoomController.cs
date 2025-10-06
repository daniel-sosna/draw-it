using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Controllers.Room.DTO;
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

    /// <summary>
    /// Create a new room and assign user as host
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(RoomCreateResponseDto), StatusCodes.Status201Created)]
    public IActionResult CreateRoom()
    {
        var (user, session) = ResolveUserSession();

        var room = _roomService.CreateRoom(session);

        return Created($"api/v1/host/{room.Id}", new RoomCreateResponseDto(room.Id));
    }

    /// <summary>
    /// Join existing room as a player
    /// </summary>
    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();

        _roomService.JoinRoom(roomId, session);

        return NoContent();
    }

    /// <summary>
    /// Leave room as a player
    /// </summary>
    [HttpPost("{roomId}/leave")]
    public IActionResult LeaveRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();

        _roomService.LeaveRoom(roomId, session);

        return NoContent();
    }

    /// <summary>
    /// Get room info
    /// * It is temporary endpoint. In the future access to the room info must be restricted!
    /// </summary>
    [HttpGet("{roomId}")]
    [AllowAnonymous]
    public IActionResult GetRoom(string roomId)
    {
        var room = _roomService.GetRoom(roomId);

        return Ok(room);
    }

    /// <summary>
    /// Delete room (host only)
    /// </summary>
    [HttpDelete("{roomId}")]
    public IActionResult DeleteRoom(string roomId)
    {
        var (user, session) = ResolveUserSession();

        var room = _roomService.GetRoom(roomId);
        if (room.HostId != user.Id)
            return Forbid();

        _roomService.DeleteRoom(roomId);

        return NoContent();
    }
}
