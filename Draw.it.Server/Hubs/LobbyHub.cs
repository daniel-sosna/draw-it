using Draw.it.Server.Extensions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

[Authorize]
public class LobbyHub : Hub
{
    private readonly IRoomService _roomService;
    private readonly IUserService _userService;
    private readonly ILogger<LobbyHub> _logger;

    public LobbyHub(IRoomService roomService, IUserService userService, ILogger<LobbyHub> logger)
    {
        _roomService = roomService;
        _userService = userService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.ResolveUser(_userService);

        if (string.IsNullOrEmpty(user.RoomId))
        {
            _logger.LogWarning("User with id={UserId} has no RoomId on connection.", user.Id);
            Context.Abort();  // Immediately close the connection
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, user.RoomId);

        var settings = _roomService.GetRoomSettings(user.RoomId);
        await Clients.Caller.SendAsync("ReceiveUpdateSettings",
            settings.CategoryId,
            settings.DrawingTime,
            settings.NumberOfRounds,
            settings.RoomName);

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to room {RoomId}", user.Id, user.RoomId);

        await SendPlayerListUpdate(user.RoomId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.ResolveUser(_userService);

        _logger.LogInformation("User with id={UserId} disconnecting... Exception: {Ex}", user.Id, exception?.Message);

        // Safely validate the user ID
        try
        {
            string? roomId = user.RoomId;
            if (!string.IsNullOrEmpty(roomId))
            {
                if (_roomService.IsHost(roomId, user))
                {
                    // If the user is the host, delete the room
                    _roomService.DeleteRoom(roomId, user);
                    _logger.LogInformation("Disconnected: host with id={UserId}. Room {RoomId} deleted.", user.Id, roomId);
                    await Clients.Group(roomId).SendAsync("ReceiveRoomDeleted");
                }
                else
                {
                    // If the user is not the host, just leave the room
                    _roomService.LeaveRoom(roomId, user);
                    _logger.LogInformation("Disconnected: user with id={UserId} left room {RoomId}.", user.Id, roomId);

                    await SendPlayerListUpdate(roomId);
                }
            }

            // Broadcast the change to the remaining users in the room
            // await Clients.Group(user.RoomId).SendAsync("ReceivePlayerLeft", user.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnDisconnectedAsync cleanup for user with id={UserId}.", user.Id);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateRoomSettings(string roomId, RoomSettingsModel settings)
    {
        var user = Context.ResolveUser(_userService);

        _logger.LogInformation("User with id={UserId} is updating settings for room {RoomId}", user.Id, roomId);
        await Task.Run(() => _roomService.UpdateSettings(roomId, user, settings));
        await Clients.Group(roomId).SendAsync("ReceiveUpdateSettings", settings.CategoryId, settings.DrawingTime, settings.NumberOfRounds, settings.RoomName);
    }

    private async Task SendPlayerListUpdate(string roomId)
    {
        var players = _roomService.GetUsersInRoom(roomId).Select(p => new
        {
            p.Name,
            p.IsReady
        }).ToList();

        await Clients.Group(roomId).SendAsync("ReceivePlayerList", players);
    }
}