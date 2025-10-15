using Draw.it.Server.Services.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class LobbyHub : Hub
{
    private readonly LobbyHubService _lobbyHubService;

    public LobbyHub(LobbyHubService lobbyHubService)
    {
        _lobbyHubService = lobbyHubService;
    }
    
    public async Task joinRoomGroup(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        // Maybe tell the service that a user has joined (for state/database).
        // await _lobbyHubService.UserJoinedLobby(Context.UserIdentifier, roomId);
        
        // Maybe add a message that a user has joined
        // await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
    }
    
}