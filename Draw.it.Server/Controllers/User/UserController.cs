using Draw.it.Server.Controllers.User.DTO;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace Draw.it.Server.Controllers.User;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("generate-id")]
    public IActionResult GenerateId()
    {
        var id = _userService.GenerateUserId();
        var response = new GenerateUserIdResponseDto(id);
        return Ok(response);
    }
    
    [HttpGet("")]
    public IActionResult GetUsers()
    {
        var ids = _userService.GetActiveUserIds();
        var response = new GetUsersResponseDto(ids);
        return Ok(response);
    }
}