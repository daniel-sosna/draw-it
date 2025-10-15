using Draw.it.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Services.Hub;

public class GameplayHubService
{
    private IHubContext<GameplayHub> _gameplayContext;

    public GameplayHubService(IHubContext<GameplayHub> gameplayContext)
    {
        _gameplayContext = gameplayContext;
    }
}