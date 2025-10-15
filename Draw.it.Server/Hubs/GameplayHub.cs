using Draw.it.Server.Services.Hub;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class GameplayHub : Hub
{
    private readonly GameplayHubService _gameplayHubService;

    public GameplayHub(GameplayHubService gameplayHubService)
    {
        _gameplayHubService = gameplayHubService;
    }

    public async Task joinGameGroup(string user, string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task sendMessage(string user, string message)
    {
        // send message
    }

}