using Draw.it.Server.Exceptions;
using Draw.it.Server.Extensions;
using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

/// <summary>
/// Hub for connecting players to rooms and lobby-related real-time updates.
/// </summary>
[Authorize]
public class LobbyHub : BaseHub<LobbyHub>
{
    private readonly IRoomService _roomService;

    public LobbyHub(ILogger<LobbyHub> logger, IRoomService roomService, IUserService userService)
        : base(logger, userService)
    {
        _roomService = roomService;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;

        await AddConnectionToRoomGroupAsync(user);
        _userService.SetConnectedStatus(user.Id, true);

        // If the user is not the host, send them the current room settings
        if (!_roomService.IsHost(roomId, user))
        {
            var settings = _roomService.GetRoomSettings(roomId);
            await Clients.Caller.SendAsync("ReceiveUpdateSettings", new SettingsDto(settings));
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to room {RoomId}", user.Id, user.RoomId);

        await SendPlayerListUpdate(roomId);
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
                await _roomService.HandleUserDisconnectionAsync(user.Id, exception);
        });

        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateRoomSettings(RoomSettingsModel settings)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var updated = false;

        await Task.Run(() => updated = _roomService.UpdateSettings(roomId, user, settings));
        _logger.LogInformation("User with id={UserId} updated settings for room {RoomId}", user.Id, roomId);

        if (!updated)
        {
            return;
        }

        await Clients.Group(roomId).SendAsync("ReceiveUpdateSettings", new SettingsDto(settings));
    }

    public async Task SendPlayerListUpdate(string roomId)
    {
        var players = _roomService.GetUsersInRoom(roomId).Select(p => new PlayerDto(p, _roomService.IsHost(roomId, p))).ToList();

        await Clients.Group(roomId).SendAsync("ReceivePlayerList", players);
    }

    public async Task SetPlayerReady(bool isReady)
    {
        var user = await ResolveUserAsync();

        _userService.SetReadyStatus(user.Id, isReady);

        await SendPlayerListUpdate(user.RoomId!);
    }

    public async Task StartGame()
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;

        try
        {
            _roomService.StartGame(roomId, user);
        }
        catch (AppException ex)
        {
            await Clients.Caller.SendAsync("ReceiveErrorOnGameStart", ex.Message);
            return;
        }
        catch (Exception)
        {
            await Clients.Caller.SendAsync("ReceiveErrorOnGameStart", "An unexpected error occurred while trying to start the game.");
        }

        await Clients.Group(roomId).SendAsync("ReceiveGameStart");
    }
}