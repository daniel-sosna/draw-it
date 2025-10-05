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

    [HttpPost("")]
    public IActionResult CreateUser([FromBody] CreateUserRequestDto request)
    {
        var user = _userService.CreateUser(request.Name);
        return new ObjectResult(user) { StatusCode = StatusCodes.Status201Created };
    }

    [HttpGet("{userId}")]
    public IActionResult GetUser([FromRoute] long userId)
    {
        var user = _userService.GetUser(userId);
        return Ok(user);
    }
}