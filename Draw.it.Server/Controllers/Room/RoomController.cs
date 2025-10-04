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

    [HttpPost("generate-id")]
    public IActionResult GenerateId()
    {
        string roomId = _roomService.GenerateUniqueRoomId();

        var response = new GenerateRoomIdResponseDto(roomId);
        return Ok(response);
    }
    
    [HttpPost("{roomId}/join")]
    public IActionResult JoinRoom([FromRoute] string roomId, [FromBody] JoinRoomRequestDto request)
    {
        var user = _userService.FindUserById(request.UserId);
        var room = _roomService.AddPlayerToRoom(roomId, user, request.IsHost);
        
        return Ok(room); 
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
        if (!_roomService.CanStartGame(roomId))
        {
            return BadRequest(new { error = "Game cannot be started. Not enough players or not ready." });
        }
        
        var room = _roomService.StartGame(roomId);
        return Ok(room);
    }
    
    [HttpPut("{roomId}/settings")]
    public IActionResult UpdateRoomSettings([FromRoute] string roomId, [FromBody] RoomSettingsModel settings)
    {
        _roomService.UpdateRoomSettings(roomId, settings);

        return Ok(settings);
    }
    
    [HttpPost("join/{roomId}/{userId}")]
    public IActionResult JoinRoom([FromRoute] string roomId, [FromRoute] long userId)
    {
        var user = _userService.FindUserById(userId); 
        
        _roomService.JoinRoom(roomId, user); 

        return Ok(); 
    }
}
