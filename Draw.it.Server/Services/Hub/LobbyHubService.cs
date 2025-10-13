using Draw.it.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Services.Hub;


// This is the lobby service for the signalR connection
public class LobbyHubService
{
    private readonly IHubContext<LobbyHub> _lobbyContext;

    public LobbyHubService(IHubContext<LobbyHub> lobbyContext)
    {
        _lobbyContext = lobbyContext;
    }
    

}