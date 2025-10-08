using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly IUserService _userService;

    protected BaseController(IUserService userService)
    {
        _userService = userService;
    }

    // Helper to get current user's ID from claims
    protected long ResolveUserId()
    {
        var userId = (User.FindFirst("userId")?.Value) ?? throw new UnauthorizedUserException("User ID claim missing.");

        return long.Parse(userId);
    }

    // Helper to get current user from claims
    protected UserModel ResolveUser()
    {
        var userId = ResolveUserId();
        var user = _userService.GetUser(userId);

        return user;
    }
}
