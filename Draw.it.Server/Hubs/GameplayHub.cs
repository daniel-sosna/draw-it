using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class GameplayHub : Hub
{
    public async Task joinGameGroup(string user, string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task sendMessage(string user, string message)
    {
        // send message
    }

}