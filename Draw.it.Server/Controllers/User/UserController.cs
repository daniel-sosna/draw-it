using System.Security.Claims;
using Draw.it.Server.Extensions;
using Draw.it.Server.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Draw.it.Server.Controllers.User;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // Additional user-related endpoints can be added here
    // For example, fetching user details, updating user info, customizing user settings, etc.
    
    /// <summary>
    /// Updates username and reissues the authentication cookie to refresh session.
    /// </summary>
    [HttpPut("newName")]
    public async Task<IActionResult> UpdateName([FromBody] Dictionary<string, string> request)
    {
        if (!request.TryGetValue("name", out var newName) || string.IsNullOrWhiteSpace(newName))
        {
            return BadRequest("New name is required.");
        }
        
        var user = HttpContext.ResolveUser(_userService);
        
        _userService.UpdateName(user.Id, newName);
        
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
    
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
            }
        );

        return NoContent(); 
    }
}
