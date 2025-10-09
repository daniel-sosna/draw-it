using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Controllers.Room.DTO;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Room;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomController : BaseController
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService, IUserService userService)
        : base(userService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Create a new room and assign user as host
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(RoomCreateResponseDto), StatusCodes.Status201Created)]
    public IActionResult CreateRoom()
    {
        var user = ResolveUser();

        var room = _roomService.CreateRoom(user);

        return Created($"api/v1/host/{room.Id}", new RoomCreateResponseDto(room.Id));
    }

    /// <summary>
    /// Join existing room as a player
    /// </summary>
    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom(string roomId)
    {
        var user = ResolveUser();

        _roomService.JoinRoom(roomId, user);

        return NoContent();
    }

    /// <summary>
    /// Leave room as a player
    /// </summary>
    [HttpPost("{roomId}/leave")]
    public IActionResult LeaveRoom(string roomId)
    {
        var user = ResolveUser();

        _roomService.LeaveRoom(roomId, user);

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
        var user = ResolveUser();

        _roomService.DeleteRoom(roomId, user);

        return NoContent();
    }

    /// <summary>
    /// Get all users currently in the room
    /// </summary>
    [HttpGet("{roomId}/users")]
    public IActionResult GetRoomUsers(string roomId)
    {
        _roomService.GetRoom(roomId);

        var users = _userService.GetUsersInRoom(roomId);

        return Ok(users);
    }
}
