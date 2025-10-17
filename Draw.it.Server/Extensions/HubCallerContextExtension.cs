using Microsoft.AspNetCore.SignalR;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Extensions;

public static class HubCallerContextExtension
{
    // Extension method to get current user's ID from HubCallerContext
    public static long ResolveUserId(this HubCallerContext context)
    {
        var stringUserId = context.UserIdentifier ?? throw new HubException("UserIdentifier missing.");

        if (!long.TryParse(stringUserId, out long userId))
        {
            throw new HubException("UserIdentifier is not a valid number.");
        }

        return userId;
    }

    // Extension method to get current user from HubCallerContext 
    public static UserModel ResolveUser(this HubCallerContext context, IUserService userService)
    {
        var userId = context.ResolveUserId();
        var user = userService.GetUser(userId);

        return user;
    }
}
