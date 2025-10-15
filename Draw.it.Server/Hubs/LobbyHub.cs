using Draw.it.Server.Extensions;
using Draw.it.Server.Services.Hub;
using Draw.it.Server.Services.Room;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class LobbyHub : Hub
{
    private readonly IRoomService _roomService;

    public LobbyHub(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task joinRoomGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        // Maybe tell the service that a user has joined (for state/database).
        // await _lobbyHubService.UserJoinedLobby(Context.UserIdentifier, roomId);

        // Maybe add a message that a user has joined
        // await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
    }

    public async Task leaveRoom(string roomId)
    {
        await _roomService.HandlePlayerLeave(Context.UserIdentifier, roomId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }
    
}