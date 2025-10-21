using Draw.it.Server.Extensions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

public class GameplayHub : Hub
{
    private readonly IUserService _userService;
    private readonly ILogger<GameplayHub> _logger;


    public GameplayHub(IUserService userService, ILogger<GameplayHub> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.ResolveUser(_userService);

        if (string.IsNullOrEmpty(user.RoomId))
        {
            _logger.LogWarning("User with id={UserId} has no RoomId on connection.", user.Id);
            Context.Abort();  // Close the connection
            return;
        }

        // Add player to a group, again
        await Groups.AddToGroupAsync(Context.ConnectionId, user.RoomId);
        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to gameplay room with roomId={RoomId}", user.Id, user.RoomId);
    }
    public async Task SendMessage(string message)
    {
        var userName = "";
        await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", userName, message);
    }

}