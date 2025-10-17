using Draw.it.Server.Extensions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Get the user id
        var user = Context.ResolveUser(_userService);

        _logger.LogInformation("User with id={UserId} disconnected. Exception: {Ex}", user.Id, exception?.Message);

        // Safely validate the user ID
        try
        {
            string? currentRoomId = user.RoomId;

            if (!string.IsNullOrEmpty(currentRoomId))
            {
                // Add this flag because the user left abruptly
                _roomService.LeaveRoom(currentRoomId, user, unexpectedLeave: true);

                // Broadcast the change to the remaining users in the room
                // await Clients.Group(currentRoomId).SendAsync("ReceivePlayerLeft", user.Name);

                _logger.LogInformation("User with id={UserId} cleaned up from room {RoomId}.", user.Id, currentRoomId);
            }

            // Delete the user
            _userService.DeleteUser(user.Id);
        }
        catch (Exception ex)
        {
            // Log any errors
            _logger.LogError(ex, "Error during OnDisconnectedAsync cleanup for user with id={UserId}.", user.Id);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoomGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Maybe add a message that a user has joined
        // var user = Context.ResolveUser(_userService);
        // await Clients.Group(roomId).SendAsync("UserJoined", user.Name);
    }

    public async Task LeaveRoom(string roomId)
    {
        var user = Context.ResolveUser(_userService);
        _roomService.LeaveRoom(roomId, user);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        _logger.LogInformation("User with id={UserId} successfully left room {RoomId}. The ConnectionId={ConnectionId}", user.Id, roomId, Context.ConnectionId);
    }

    public async Task UpdateRoomSettings(string roomId, RoomSettingsModel settings)
    {
        _logger.LogInformation("User with ConnectionId={ConnectionId} is updating room settings {settings}", Context.ConnectionId, settings);
        await Task.Run(() => _roomService.UpdateSettings(roomId, Context.ResolveUser(_userService), settings));
        await Clients.Group(roomId).SendAsync("ReceiveUpdateSettings", settings.CategoryId, settings.DrawingTime, settings.NumberOfRounds, settings.RoomName);
    }

    public async Task RequestSettingsUpdate(string roomId)
    {
        await Clients.Group(roomId).SendAsync("RequestCurrentSettings");
    }
}