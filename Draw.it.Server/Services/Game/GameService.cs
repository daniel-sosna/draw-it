using System.Net;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Game;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Enums;
using Draw.it.Server.Services.WordPool;


namespace Draw.it.Server.Services.Game;

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IGameRepository _gameRepository;
    private readonly IRoomService _roomService;
    private readonly IWordPoolService _wordPoolService;

    public GameService(ILogger<GameService> logger, IGameRepository gameRepository, IRoomService roomService, IWordPoolService wordPoolService)
    {
        _logger = logger;
        _gameRepository = gameRepository;
        _roomService = roomService;
        _wordPoolService = wordPoolService;
    }

    public void CreateGame(string roomId)
    {
        var room = _roomService.GetRoom(roomId);

        if (room.Status != RoomStatus.InGame)
        {
            throw new AppException($"Cannot start game for room {roomId} because the room status is not 'InGame'.", HttpStatusCode.Conflict);
        }

        var game = new GameModel
        {
            RoomId = roomId,
            PlayerCount = _roomService.GetUsersInRoom(roomId).Count(),
            CurrentDrawerId = GetPlayerIdByTurnIndex(roomId, 0),
            WordToDraw = GetRandomWord(room.Settings.CategoryId)
        };

        _gameRepository.Save(game);
        _logger.LogInformation("Game for room id={roomId} created. First drawer: {drawerId}, Word: {word}", roomId, game.CurrentDrawerId, game.WordToDraw);
    }

    public void DeleteGame(string roomId)
    {
        if (!_gameRepository.DeleteById(roomId))
        {
            _logger.LogWarning("Attempted to delete non-existent game for room id={roomId}", roomId);
        }

        _gameRepository.DeleteById(roomId);
    }

    public GameModel GetGame(string roomId)
    {
        return _gameRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Game for room id={roomId} not found");
    }

    public long GetDrawerId(string roomId)
    {
        return GetGame(roomId).CurrentDrawerId;
    }

    // TODO: delete
    public void SetDrawerId(string roomId, long newDrawerId)
    {
        var game = GetGame(roomId);

        game.CurrentDrawerId = newDrawerId;

        _gameRepository.Save(game);
    }

    public bool AddGuessedPlayer(string roomId, long userId)
    {
        var game = GetGame(roomId);

        // Drawer cannot guess
        if (userId == game.CurrentDrawerId) return false;

        if (game.GuessedPlayersIds.Contains(userId)) return false;

        // Determine points: first correct guess gets max (equal to total players), then decreases
        var position = game.GuessedPlayersIds.Count; // 0-based
        var points = Math.Max(1, game.PlayerCount - position);

        // Increment correct guesses (persistent across rounds)
        if (game.CorrectGuesses.ContainsKey(userId))
            game.CorrectGuesses[userId] += 1;
        else
            game.CorrectGuesses[userId] = 1;

        // Add points for this round (cleared at end of round)
        if (game.RoundScores.ContainsKey(userId))
            game.RoundScores[userId] += points;
        else
            game.RoundScores[userId] = points;

        game.GuessedPlayersIds.Add(userId);

        _gameRepository.Save(game);

        return game.GuessedPlayersIds.Count >= game.PlayerCount - 1;
    }

    // TODO: delete
    public void ClearGuessedPlayers(string roomId)
    {
        var game = GetGame(roomId);

        game.GuessedPlayersIds.Clear();

        _gameRepository.Save(game);
    }


    public bool AdvanceTurn(string roomId)
    {
        var game = GetGame(roomId);
        var room = _roomService.GetRoom(roomId);

        var nextDrawerId = GetNextDrawerId(game);

        if (nextDrawerId == -1)
        {
            _gameRepository.Save(game);
            return true;
        }

        game.CurrentDrawerId = nextDrawerId;
        game.WordToDraw = GetRandomWord(room.Settings.CategoryId);
        ClearGuessedPlayers(roomId);

        _gameRepository.Save(game);

        return false;
    }

    public string GetMaskedWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return string.Empty;

        return new string(word.Select(c => char.IsWhiteSpace(c) ? ' ' : '*').ToArray());
    }

    public string GetRandomWord(long categoryId)
    {
        var randomWord = _wordPoolService.GetRandomWordByCategoryId(categoryId);
        return randomWord.Value;
    }

    private long GetPlayerIdByTurnIndex(string roomId, int turnIndex)
    {
        return _roomService.GetUsersInRoom(roomId).Select(p => p.Id).ElementAt(turnIndex);
    }

    private long GetNextDrawerId(GameModel game)
    {
        var totalRounds = _roomService.GetRoom(game.RoomId).Settings.NumberOfRounds;
        var nextTurnIndex = (game.CurrentTurnIndex + 1) % game.PlayerCount;

        if (nextTurnIndex == 0)
        {
            var newRoundValue = game.CurrentRound + 1;
            if (newRoundValue > totalRounds)
            {
                return -1;
            }

            game.CurrentRound = newRoundValue;
        }

        game.CurrentTurnIndex = nextTurnIndex;

        _gameRepository.Save(game);

        return GetPlayerIdByTurnIndex(game.RoomId, game.CurrentTurnIndex);
    }
}