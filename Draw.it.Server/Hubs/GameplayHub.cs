using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Draw.it.Server.Hubs;

/// <summary>
/// Hub for gameplay-related real-time communication.
/// </summary>
[Authorize]
public class GameplayHub : BaseHub<GameplayHub>
{
    public GameplayHub(ILogger<GameplayHub> logger, IUserService userService)
        : base(logger, userService)
    {
    }

    public override async Task OnConnectedAsync()
    {
        var user = await ResolveUserAsync();

        await AddConnectionToRoomGroupAsync(user);

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to gameplay room with roomId={RoomId}", user.Id, user.RoomId);
    }

    public async Task SendMessage(string message)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;

        await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync(method: "ReceiveMessage", arg1: user.Name, arg2: message);
        // Later on maybe implement a saving messages method in some database
    }

    public async Task SendDraw(DrawDto drawDto)
    {
        var user = await ResolveUserAsync();
        await Clients.GroupExcept(user.RoomId!, Context.ConnectionId).SendAsync(method: "ReceiveDraw", arg1: drawDto);
    }

    public async Task SendClear()
    {
        var user = await ResolveUserAsync();
        await Clients.GroupExcept(user.RoomId!, Context.ConnectionId).SendAsync(method: "ReceiveClear");
    }
}