using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Controllers.Room.DTO;
using Draw.it.Server.Extensions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Room;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public RoomController(IRoomService roomService, IUserService userService)
    {
        _roomService = roomService;
        _userService = userService;
    }

    /// <summary>
    /// Create a new room and assign user as host
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(RoomCreateResponseDto), StatusCodes.Status201Created)]
    public IActionResult CreateRoom()
    {
        var user = HttpContext.ResolveUser(_userService);

        var room = _roomService.CreateRoom(user);

        return Created($"api/v1/host/{room.Id}", new RoomCreateResponseDto(room.Id));
    }

    /// <summary>
    /// Join existing room as a player
    /// </summary>
    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom(string roomId)
    {
        var user = HttpContext.ResolveUser(_userService);

        _roomService.JoinRoom(roomId, user);

        return NoContent();
    }

    /// <summary>
    /// Leave room as a player
    /// </summary>
    [HttpPost("{roomId}/leave")]
    public IActionResult LeaveRoom(string roomId)
    {
        var user = HttpContext.ResolveUser(_userService);

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
        var user = HttpContext.ResolveUser(_userService);

        _roomService.DeleteRoom(roomId, user);

        return NoContent();
    }

    /// <summary>
    /// Get all users currently in the room
    /// </summary>
    [HttpGet("{roomId}/players")]
    public IActionResult GetRoomUsers(string roomId)
    {
        var users = _roomService.GetUsersInRoom(roomId);

        return Ok(users);
    }

    /// <summary>
    /// Start the game
    /// </summary>
    [HttpPost("{roomId}/start")]
    public IActionResult StartGame(string roomId)
    {
        var user = HttpContext.ResolveUser(_userService);

        _roomService.StartGame(roomId, user);

        return NoContent();
    }

    /// <summary>
    /// Updates room settings (Host only)
    /// </summary>
    [HttpPut("{roomId}/settings")]
    public IActionResult UpdateSettings(
        string roomId,
        [FromBody] RoomSettingsModel newSettings)
    {
        var user = HttpContext.ResolveUser(_userService);

        _roomService.UpdateSettingsInternal(roomId, user, newSettings);

        return NoContent();
    }
}
