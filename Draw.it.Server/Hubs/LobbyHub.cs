using Draw.it.Server.Exceptions;
using Draw.it.Server.Extensions;
using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Services.Game;
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
    private readonly IGameService _gameService;

    public LobbyHub(ILogger<LobbyHub> logger, IRoomService roomService, IUserService userService, IGameService gameService)
        : base(logger, userService, roomService)
    {
        _gameService = gameService;
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

        _logger.LogInformation("User with id={UserId} disconnecting... Exception:\n{Ex}", user.Id, exception?.Message);
        _userService.SetConnectedStatus(user.Id, false);

        // Broadcast the change to other users in the room
        // await Clients.Group(user.RoomId).SendAsync("ReceivePlayerDisconnected", user.Name);

        // If user is still in a room (unintended disconnection) wait a bit for reconnection
        if (!string.IsNullOrEmpty(user.RoomId))
        {
            await Task.Run(async () =>
            {
                await Task.Delay(8000);
                if (!user.IsConnected)
                    await LeaveRoom();
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task LeaveRoom()
    {
        var user = await ResolveUserAsync();
        string? roomId = user.RoomId;

        if (string.IsNullOrEmpty(roomId))
        {
            return;
        }

        try
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
        catch (AppException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during HandleUserDisconnection for user with id={UserId}:\n{Ex}", user.Id, ex);
            throw new HubException("An unexpected error occurred while trying to leave the room.");
        }
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
            await Task.Run(() => _roomService.StartGame(roomId, user));

            await Task.Run(() => _gameService.CreateGame(roomId));
        }
        catch (AppException ex)
        {
            await Clients.Caller.SendAsync("ReceiveErrorOnGameStart", ex.Message);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while trying to start the game:\n{Ex}", ex);
            await Clients.Caller.SendAsync("ReceiveErrorOnGameStart", "An unexpected error occurred while trying to start the game.");
        }

        await Clients.Group(roomId).SendAsync("ReceiveGameStart");
    }
}