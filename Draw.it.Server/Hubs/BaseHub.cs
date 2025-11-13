using Draw.it.Server.Extensions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

/// <summary>
/// Base hub that contains shared functionality for hubs that work with users and rooms.
/// </summary>
public abstract class BaseHub<T> : Hub where T : Hub
{
    protected readonly ILogger<T> _logger;
    protected readonly IUserService _userService;
    protected readonly IRoomService _roomService;

    protected BaseHub(ILogger<T> logger, IUserService userService, IRoomService roomService)
    {
        _logger = logger;
        _userService = userService;
        _roomService = roomService;

    }

    /// <summary>
    /// Resolve the current user from the hub context using the IUserService and ensure they have a RoomId.
    /// </summary>
    protected async Task<UserModel> ResolveUserAsync()
    {
        var user = Context.ResolveUser(_userService);

        await CheckUserInRoomAsync(user);

        return user;
    }

    /// <summary>
    /// Add the current connection to the SignalR group by the user's room.
    /// </summary>
    protected async Task AddConnectionToRoomGroupAsync(UserModel user)
    {
        await CheckUserInRoomAsync(user);

        await Groups.AddToGroupAsync(Context.ConnectionId, user.RoomId!);
    }

    /// <summary>
    /// Check if the user has a RoomId; if not, abort the connection and notify the client.
    /// </summary>
    protected async Task CheckUserInRoomAsync(UserModel user)
    {
        if (string.IsNullOrEmpty(user.RoomId))
        {
            _logger.LogWarning("User with id={UserId} has no RoomId.", user.Id);
            await Clients.Caller.SendAsync("ReceiveConnectionAborted", "You are not in a room.");
            Context.Abort();
            throw new HubException("User has no RoomId.");
        }
    }
}
