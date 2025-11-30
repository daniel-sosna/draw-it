using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Models.User;
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
    private const int RoundDelayMs = 6000;
    private const int EndGameDelayMs = 10000;

    public GameplayHub(ILogger<GameplayHub> logger, IUserService userService, IGameService gameService, IRoomService roomService)
        : base(logger, userService, roomService)
    {
        _gameService = gameService;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;

        await AddConnectionToRoomGroupAsync(user);
        var added = _gameService.AddConnectedPlayer(roomId, user.Id);

        // Manage reconnection or new connection scenarios
        var game = _gameService.GetGame(roomId);

        var isDrawer = game.CurrentDrawerId == user.Id;
        await Clients.Caller.SendAsync("SetDrawerStatus", isDrawer);

        if (game.ConnectedPlayersIds.Count == game.PlayerCount)
        {
            // All players are connected - game in progress
            if (added)
            {
                await StartTurn(roomId, true);
            }
            else
            {
                var word = game.WordToDraw;
                var isDrawerOrGuessed = game.CurrentDrawerId == user.Id || game.GuessedPlayersIds.Contains(user.Id);
                await Clients.Caller.SendAsync("ReceiveWordToDraw", isDrawerOrGuessed ? word : _gameService.GetMaskedWord(word));
            }
        }
        else
        {
            // Waiting for other players to connect
            if (added)
            {
                await SendSystemMessageToRoom(roomId, $"{user.Name} joined the game");
            }
            var waitingMessage = $"Waiting for other players to connect... ({game.ConnectedPlayersIds.Count}/{game.PlayerCount})";
            await Clients.Caller.SendAsync("ReceiveMessage", "System", waitingMessage);
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("Connected: User with id={UserId} to gameplay room with roomId={RoomId}", user.Id, roomId);
    }

    public async Task SendMessage(string message)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var game = _gameService.GetGame(roomId);
        var drawerId = game.CurrentDrawerId;
        var wordToDraw = game.WordToDraw;

        var isCorrectGuess = string.Equals(message.Trim(), wordToDraw,
            StringComparison.OrdinalIgnoreCase); // check if the word is the word to guess

        if (drawerId == user.Id || !isCorrectGuess)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, message);
            return;
        }

        await Clients.Caller.SendAsync("ReceiveWordToDraw", wordToDraw);
        await Clients.Group(roomId).SendAsync("ReceiveMessage", user.Name, "Guessed The Word!", true);

        _gameService.AddGuessedPlayer(roomId, user.Id, out bool turnEnded, out bool roundEnded, out bool gameEnded);

        if (turnEnded) await ManageTurnEnding(roomId, wordToDraw, roundEnded, gameEnded);
    }

    public async Task SendDraw(DrawDto drawDto)
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var game = _gameService.GetGame(roomId);

        if (user.Id != game.CurrentDrawerId)
        {
            return;
        }

        await Clients.GroupExcept(user.RoomId!, Context.ConnectionId).SendAsync("ReceiveDraw", drawDto);
    }

    public async Task SendClear()
    {
        var user = await ResolveUserAsync();
        var roomId = user.RoomId!;
        var game = _gameService.GetGame(roomId);

        if (user.Id != game.CurrentDrawerId)
        {
            return;
        }

        await Clients.GroupExcept(user.RoomId!, Context.ConnectionId).SendAsync("ReceiveClear");
    }

    private async Task SendSystemMessageToRoom(string roomId, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", message);
    }

    private async Task ManageTurnEnding(string roomId, string wordToDraw, bool roundEnded, bool gameEnded)
    {
        await EndTurn(roomId, wordToDraw);
        await Task.Delay(TurnDelayMs);

        if (gameEnded)
        {
            await EndGame(roomId);
            return;
        }

        if (roundEnded)
        {
            await EndRound(roomId);
            await Task.Delay(RoundDelayMs);
            await StartTurn(roomId, true);
        }
        else
        {
            await StartTurn(roomId);
        }
    }

    private async Task StartTurn(string roomId, bool isFirstTurn = false)
    {
        var game = _gameService.GetGame(roomId);
        var maskedWord = _gameService.GetMaskedWord(game.WordToDraw);
        var drawerId = game.CurrentDrawerId.ToString();
        var drawerName = _userService.GetUser(game.CurrentDrawerId).Name;

        await Clients.Group(roomId).SendAsync("ReceiveClear");
        await Clients.Group(roomId).SendAsync("ReceiveTurnStarted");

        if (isFirstTurn) await StartRound(roomId);

        var turnMessage = $"{drawerName} is drawing!";
        await SendSystemMessageToRoom(roomId, turnMessage);

        await Clients.GroupExcept(roomId, drawerId).SendAsync("ReceiveWordToDraw", maskedWord);
        await Clients.User(drawerId).SendAsync("ReceiveWordToDraw", game.WordToDraw);

        foreach (var playerId in game.ConnectedPlayersIds)
        {
            var isDrawer = playerId == game.CurrentDrawerId;
            await Clients.User(playerId.ToString()).SendAsync("SetDrawerStatus", isDrawer);
        }
    }

    private async Task EndTurn(string roomId, string wordToDraw)
    {
        var endMessage = $"TURN ENDED! The word was: {wordToDraw}";
        await SendSystemMessageToRoom(roomId, endMessage);
    }

    private async Task StartRound(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        var game = _gameService.GetGame(roomId);
        var totalRounds = room.Settings.NumberOfRounds;

        await Clients.Group(roomId).SendAsync("ReceiveRoundStarted", game.CurrentRound);

        var roundMessage = $"ROUND {game.CurrentRound}/{totalRounds} STARTED!";
        await SendSystemMessageToRoom(roomId, roundMessage);
    }

    private async Task EndRound(string roomId)
    {
        var game = _gameService.GetGame(roomId);
        var players = _roomService.GetUsersInRoom(roomId);
        var scores = ConvertScoresToDto(players, game.TotalScores);

        await Clients.Group(roomId).SendAsync("ReceiveRoundEnded", scores);
    }

    private async Task EndGame(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        var game = _gameService.GetGame(roomId);
        var totalRounds = room.Settings.NumberOfRounds;
        var players = _roomService.GetUsersInRoom(roomId);
        var scores = ConvertScoresToDto(players, game.TotalScores);

        var endMessage = $"GAME FINISHED! All {totalRounds} rounds played.";
        await SendSystemMessageToRoom(roomId, endMessage);

        await Clients.Group(roomId).SendAsync("ReceiveGameEnded", scores);

        await Task.Delay(EndGameDelayMs);

        _userService.RemoveRoomFromAllUsers(roomId);
        _gameService.DeleteGame(roomId);
        await Clients.Group(roomId).SendAsync("ReceiveConnectionAborted", "Game has ended. Returning to lobby.");
    }

    private List<ScoreDto> ConvertScoresToDto(IEnumerable<UserModel> users, Dictionary<long, int> scores)
    {
        var scoreDtos = new List<ScoreDto>();

        foreach (var user in users)
        {
            var exists = scores.TryGetValue(user.Id, out int points);
            if (!exists) points = 0;
            scoreDtos.Add(new ScoreDto(user.Name, points));
        }

        return scoreDtos.OrderByDescending(s => s.Points).ToList();
    }
}