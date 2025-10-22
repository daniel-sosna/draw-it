using System.Security.Claims;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Extensions;

public static class HttpContextExtension
{
    // Extension method to get current user's ID from claims in HttpContext
    public static long ResolveUserId(this HttpContext context)
    {
        var stringUserId = (context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new UnauthorizedUserException();

        if (!long.TryParse(stringUserId, out long userId))
        {
            throw new UnauthorizedUserException("User ID claim is not a valid number.");
        }

        return userId;
    }

    // Extension method to get current user from claims in HttpContext
    public static UserModel ResolveUser(this HttpContext context, IUserService userService)
    {
        var userId = context.ResolveUserId();
        var user = userService.GetUser(userId);

        return user;
    }
}
