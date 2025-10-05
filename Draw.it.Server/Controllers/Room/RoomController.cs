using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Controllers.Room.DTO;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers.Room;

[ApiController]
[Route("api/v1/[controller]")]
public class RoomController : ControllerBase
{

    private readonly IRoomService _roomService;
    private readonly IUserService _userService;

    public RoomController(IRoomService roomService, IUserService userService)
    {
        _roomService = roomService;
        _userService = userService;
    }

    [HttpPost("create")]
    public IActionResult CreateRoom([FromBody] CreateRoomRequestDto request)
    {
        var user = _userService.FindUserById(request.UserId);
        var room = _roomService.CreateRoomAsHost(user);

        return Ok(room);
    }

    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom([FromRoute] string roomId, [FromBody] JoinRoomRequestDto request)
    {
        _roomService.JoinRoom(roomId, request.UserId);

        return Ok();
    }

    [HttpGet("{roomId}/status")]
    public IActionResult GetRoomStatus([FromRoute] string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        return Ok(room);
    }

    [HttpPut("{roomId}/player/{userId}/ready")]
    public IActionResult SetPlayerReadyStatus([FromRoute] string roomId, [FromRoute] long userId, [FromBody] SetReadyStatusRequestDto request)
    {
        _roomService.SetPlayerReady(roomId, userId, request.IsReady);
        return Ok();
    }

    [HttpPut("{roomId}/start")]
    public IActionResult StartGame([FromRoute] string roomId, [FromQuery] long hostUserId)
    {
        var room = _roomService.StartGame(roomId);

        return Ok(room);
    }

    [HttpPatch("{roomId}/settings")]
    public IActionResult UpdateRoomSettings([FromRoute] string roomId, [FromBody] PatchRoomSettingsDto settingsPatch)
    {
        _roomService.UpdateRoomSettings(roomId, settingsPatch);

        return Ok();
    }

}
