using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
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

    private const int TurnDelayMs = 3000;
    private const int RoundDelayMs = 5000;

    public GameplayHub(ILogger<GameplayHub> logger, IUserService userService, IGameService gameService, IRoomService roomService)
        : base(logger, userService, roomService)
    {
        _gameService = gameService;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await ResolveUserAsync();

        await AddConnectionToRoomGroupAsync(user);

        var game = _gameService.GetGame(user.RoomId!);

        if (game.CurrentDrawerId == user.Id)
        {
            await SendWord();
        }
        else
        {
            string maskedWord = _gameService.GetMaskedWord(game.WordToDraw);
            await Clients.Caller.SendAsync(method: "ReceiveWordToDraw", arg1: maskedWord);
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to gameplay room with roomId={RoomId}", user.Id, user.RoomId);
    }

    public async Task SendMessage(string message)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var drawerId = _gameService.GetDrawerId(roomId);

        var isCorrectGuess = string.Equals(message.Trim(), _gameService.GetGame(roomId).WordToDraw,
            StringComparison.OrdinalIgnoreCase); // check if the word is the word to guess

        if (drawerId == user.Id || !isCorrectGuess) {
            await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: user.Name, arg2: message, arg3: false);
            return;
        }

        await SendWord(correctGuess: true);
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: user.Name, arg2: "Guessed The Word!", arg3: true);

        _gameService.AddGuessedPlayer(roomId, user.Id, out bool turnEnded, out bool roundEnded, out bool gameEnded);

        if (turnEnded) {
            await EndTurn(roomId);
            if (roundEnded) {
                await EndRound(roomId);
                if (gameEnded) {
                    await EndGame(roomId);
                    return;
                } else {
                    await StartRound(roomId);
                }
            }
            await StartTurn(roomId);
        }
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

    private async Task StartTurn(string roomId)
    {
        await Clients.Group(roomId).SendAsync(method: "ReceiveClear");
        await Clients.Group(roomId).SendAsync("TurnUpdate");

        var game = _gameService.GetGame(roomId);

        var maskedWord = _gameService.GetMaskedWord(game.WordToDraw);
        var drawerId = game.CurrentDrawerId.ToString();
        var drawerName = _userService.GetUser(game.CurrentDrawerId).Name;

        var turnMessage = $"{drawerName} is drawing!";
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: "System", arg2: turnMessage, arg3: false);

        await Clients.GroupExcept(roomId, drawerId).SendAsync(
            method: "ReceiveWordToDraw",
            arg1: maskedWord);

        await Clients.User(drawerId).SendAsync(
            method: "ReceiveWordToDraw",
            arg1: game.WordToDraw);
    }

    private async Task EndTurn(string roomId)
    {
        var game = _gameService.GetGame(roomId);

        var endMessage = $"TURN ENDED! The word was: {game.WordToDraw}";
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: "System", arg2: endMessage, arg3: false);

        await Task.Delay(TurnDelayMs);
    }

    private async Task StartRound(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        var game = _gameService.GetGame(roomId);
        var totalRounds = room.Settings.NumberOfRounds;

        var roundMessage = $"ROUND { game.CurrentRound }/{totalRounds} STARTED!";
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: "System", arg2: roundMessage, arg3: false);
    }

    private async Task EndRound(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        var game = _gameService.GetGame(roomId);

        var endMessage = $"ROUND ENDED... Scores:\n" +
            string.Join("\n", game.RoundScores.Select(kvp =>
                {
                    var userName = _userService.GetUser(kvp.Key).Name;
                    var score = kvp.Value;
                    return $"{userName}: {score} points";
                }));
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: "System", arg2: endMessage, arg3: false);

        await Task.Delay(RoundDelayMs);
    }

    private async Task EndGame(string roomId) {
        var room = _roomService.GetRoom(roomId);
        var totalRounds = room.Settings.NumberOfRounds;

        var endMessage = $"GAME FINISHED! All {totalRounds} rounds played.";
        await Clients.Group(roomId).SendAsync(method: "ReceiveMessage", arg1: "System", arg2: endMessage, arg3: false);

        _userService.RemoveRoomFromAllUsers(roomId);
        _gameService.DeleteGame(roomId);
        await Clients.Group(roomId).SendAsync("GameEnded");
    }
}