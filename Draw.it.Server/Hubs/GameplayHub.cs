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
        bool isCorrectGuess = false;

        if (drawerId != user.Id)
        {
            isCorrectGuess = string.Equals(message.Trim(), _gameService.GetGame(roomId).WordToDraw,
                StringComparison.OrdinalIgnoreCase); // check if the word is the word to guess
            messageToSend = isCorrectGuess ? "Guessed The Word!" : message;
            if (isCorrectGuess) await SendWord(correctGuess: true); // unhide the word
        }
        await Clients.Group(user.RoomId!).SendAsync(method: "ReceiveMessage", arg1: user.Name, arg2: messageToSend, arg3: isCorrectGuess);
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

    public async Task SendWord(bool correctGuess = false)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var userId = _gameService.GetGame(roomId).CurrentDrawerId.ToString();

        // Only send to the current drawer or to someone who guessed it
        if (correctGuess)
        {
            await Clients.Caller.SendAsync(method: "ReceiveWordToDraw", arg1: _gameService.GetGame(roomId).WordToDraw); // reveal the word to the guesser
        }
        else
        {
            await Clients.User(userId).SendAsync(method: "ReceiveWordToDraw", arg1: _gameService.GetGame(roomId).WordToDraw);
        }
        _logger.LogInformation("Sent word: {wordToDraw}", _gameService.GetGame(roomId).WordToDraw);
    }
}