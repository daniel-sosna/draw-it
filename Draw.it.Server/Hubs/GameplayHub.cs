using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Services.Game;
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
    private readonly IGameService _gameService;
    public GameplayHub(ILogger<GameplayHub> logger, IUserService userService, IGameService gameService)
        : base(logger, userService)
    {
        _gameService = gameService;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await ResolveUserAsync();
        

        await AddConnectionToRoomGroupAsync(user);
        await SendWord(); // send the initial word to the player
                          // Later the words will be sent after each round

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to gameplay room with roomId={RoomId}", user.Id, user.RoomId);
    }

    public async Task SendMessage(string message)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var drawerId = _gameService.GetGame(roomId).CurrentDrawerId;

        string messageToSend = message;

        if (drawerId != user.Id)
        {
            bool isCorrectGuess = string.Equals(message.Trim(), _gameService.GetGame(roomId).WordToDraw,
                StringComparison.OrdinalIgnoreCase); // check if it is the correct word
            messageToSend = isCorrectGuess ? "Guessed The Word!" : message;
        }
        await Clients.GroupExcept(user.RoomId!, Context.ConnectionId).SendAsync(method: "ReceiveMessage", arg1: user.Name, arg2: messageToSend);
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

    public async Task SendWord()
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var userId = _gameService.GetGame(roomId).CurrentDrawerId.ToString();

        // Only send to the current drawer
        await Clients.User(userId).SendAsync(method: "ReceiveWordToDraw", arg1: _gameService.GetGame(roomId).WordToDraw);
        _logger.LogInformation("Sent word: {wordToDraw}", _gameService.GetGame(roomId).WordToDraw);
    }

    
}