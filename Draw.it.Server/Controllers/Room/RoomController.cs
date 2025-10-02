using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Draw.it.Server.Models;
using Draw.it.Server.Services.Rooms;
using Draw.it.Server.Controllers.Rooms.DTO;

namespace Draw.it.Server.Controllers.Rooms;

[ApiController]
[Route("api/v1/[controller]")]
public class RoomsController : ControllerBase
{

    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
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



    [HttpPost("create/{roomId}")]
    public IActionResult CreateRoom([FromRoute] string roomId, [FromBody] RoomSettings settings)
    {
        _roomService.CreateAndAddRoom(roomId, settings);

        return Ok();
    }
}
