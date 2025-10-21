using Draw.it.Server.Exceptions;
using Draw.it.Server.Extensions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
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

        _userService.SetConnectedStatus(user.Id, true);
        await Groups.AddToGroupAsync(Context.ConnectionId, user.RoomId);

        // If the user is not the host, send them the current room settings
        if (!_roomService.IsHost(user.RoomId, user))
        {
            var settings = _roomService.GetRoomSettings(user.RoomId);
            await Clients.Caller.SendAsync("ReceiveUpdateSettings", new
            {
                RoomName = settings.RoomName,
                CategoryName = settings.CategoryId,
                DrawingTime = settings.DrawingTime,
                NumberOfRounds = settings.NumberOfRounds
            });
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to room {RoomId}", user.Id, user.RoomId);

        await SendPlayerListUpdate(user.RoomId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.ResolveUser(_userService);

        _userService.SetConnectedStatus(user.Id, false);

        // Broadcast the change to other users in the room
        // await Clients.Group(user.RoomId).SendAsync("ReceivePlayerDisconnected", user.Name);

        // Wait a bit for reconnection
        _ = Task.Run(async () =>
        {
            await Task.Delay(8000);
            if (!user.IsConnected)
                await HandleUserDisconnection(user, exception);
        });

        await base.OnDisconnectedAsync(exception);
    }

    private async Task HandleUserDisconnection(UserModel user, Exception? exception)
    {
        _logger.LogInformation("User with id={UserId} disconnecting... Exception:\n{Ex}", user.Id, exception?.Message);

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
            _logger.LogError(ex, "Error during HandleUserDisconnection for user with id={UserId}.", user.Id);
        }
    }

    public async Task UpdateRoomSettings(string roomId, RoomSettingsModel settings)
    {
        var user = Context.ResolveUser(_userService);

        await Task.Run(() => _roomService.UpdateSettings(roomId, user, settings));
        _logger.LogInformation("User with id={UserId} is updated settings for room {RoomId}", user.Id, roomId);

        await Clients.Group(roomId).SendAsync("ReceiveUpdateSettings", new
        {
            RoomName = settings.RoomName,
            CategoryName = settings.CategoryId, // Note: use CategoryId for now, since word pool service is not implemented yet
            DrawingTime = settings.DrawingTime,
            NumberOfRounds = settings.NumberOfRounds
        });
    }

    public async Task SendPlayerListUpdate(string roomId)
    {
        var players = _roomService.GetUsersInRoom(roomId).Select(p => new
        {
            Name = p.Name,
            IsHost = _roomService.IsHost(roomId, p),
            IsConnected = p.IsConnected,
            IsReady = p.IsReady
        }).ToList();

        await Clients.Group(roomId).SendAsync("ReceivePlayerList", players);
    }

    public async Task SetPlayerReady(bool isReady)
    {
        var user = Context.ResolveUser(_userService);

        if (string.IsNullOrEmpty(user.RoomId))
        {
            return;
        }

        _userService.SetReady(user.Id, isReady);

        await SendPlayerListUpdate(user.RoomId);
    }


    public async Task StartGame()
    {
        var user = Context.ResolveUser(_userService);

        if (string.IsNullOrEmpty(user.RoomId))
        {
            return;
        }

        try
        {
            _roomService.StartGame(user.RoomId, user);

            await Clients.Group(user.RoomId).SendAsync("ReceiveGameStart");
        }
        catch (AppException ex)
        {
            await Clients.Caller.SendAsync("ReceiveError", ex.Message);
            return;
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("ReceiveError", "An unexpected error occurred while trying to start the game.");
        }
    }
}