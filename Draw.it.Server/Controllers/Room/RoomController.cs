using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Controllers.Room.DTO;

namespace Draw.it.Server.Controllers.Room;

[ApiController]
[Route("api/v1/[controller]")]
public class RoomController : ControllerBase
{

    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("generate-id")]
    public IActionResult GenerateId()
    {
        string roomId = _roomService.GenerateUniqueRoomId();

        var response = new GenerateRoomIdResponseDto(roomId);
        return Ok(response);
    }



    [HttpPost("{roomId}")]
    public IActionResult CreateRoom([FromRoute] string roomId, [FromBody] RoomSettingsModel settings)
    {
        _roomService.CreateAndAddRoom(roomId, settings);

        return StatusCode(StatusCodes.Status201Created);
    }
}
