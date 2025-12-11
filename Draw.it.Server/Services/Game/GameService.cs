using System.Net;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Game;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Enums;
using Draw.it.Server.Hubs.DTO;
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
            throw new EntityNotFoundException($"Game for room id={roomId} not found");
        }
    }

    public GameModel GetGame(string roomId)
    {
        return _gameRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Game for room id={roomId} not found");
    }

    public long GetDrawerId(string roomId)
    {
        return GetGame(roomId).CurrentDrawerId;
    }

    public bool AddConnectedPlayer(string roomId, long userId)
    {
        var game = GetGame(roomId);
        var added = game.ConnectedPlayersIds.Add(userId);
        _gameRepository.Save(game);
        return added;
    }

    public void AddGuessedPlayer(string roomId, long userId, out bool turnEnded, out bool roundEnded, out bool gameEnded)
    {
        var game = GetGame(roomId);
        turnEnded = roundEnded = gameEnded = false;

        // Drawer cannot guess
        if (userId == game.CurrentDrawerId) return;
        // Already guessed
        if (game.GuessedPlayersIds.Contains(userId)) return;

        // Determine points: first correct guess gets max (equal to total players), then decreases
        var position = game.GuessedPlayersIds.Count;
        var points = Math.Max(1, game.PlayerCount - position);

        // Update scores
        if (!game.CorrectGuesses.TryAdd(userId, 1))
            game.CorrectGuesses[userId] += 1;
        if (!game.RoundScores.TryAdd(userId, points))
            game.RoundScores[userId] += points;

        game.GuessedPlayersIds.Add(userId);

        _gameRepository.Save(game);

        if (game.GuessedPlayersIds.Count >= game.PlayerCount - 1)
        {
            turnEnded = true;
            AdvanceTurn(game, out roundEnded, out gameEnded);
        }
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

    private void AdvanceTurn(GameModel game, out bool roundEnded, out bool gameEnded)
    {
        var room = _roomService.GetRoom(game.RoomId);
        roundEnded = gameEnded = false;

        var nextTurnIndex = (game.CurrentTurnIndex + 1) % game.PlayerCount;
        var nextDrawerId = GetPlayerIdByTurnIndex(game.RoomId, nextTurnIndex);

        game.CurrentTurnIndex = nextTurnIndex;
        game.CurrentDrawerId = nextDrawerId;
        game.WordToDraw = GetRandomWord(room.Settings.CategoryId);
        game.GuessedPlayersIds.Clear();
        game.CanvasStrokes.Clear();

        _gameRepository.Save(game);

        if (nextTurnIndex == 0)
        {
            roundEnded = true;
            AdvanceRound(game, out gameEnded);
        }
    }

    private void AdvanceRound(GameModel game, out bool gameEnded)
    {

        foreach (var kvp in game.RoundScores)
        {
            if (!game.TotalScores.TryAdd(kvp.Key, kvp.Value))
                game.TotalScores[kvp.Key] += kvp.Value;
        }
        game.CurrentRound += 1;
        game.RoundScores.Clear();

        _gameRepository.Save(game);

        var totalRounds = _roomService.GetRoom(game.RoomId).Settings.NumberOfRounds;
        gameEnded = game.CurrentRound > totalRounds;
    }

    public void AddCanvasEvent(string roomId, DrawDto drawDto)
    {
        var game = GetGame(roomId);
        if (drawDto == null) return;

        if (drawDto.Type == DrawType.Start)
        {
            var stroke = new StrokeDto(new List<Point> { drawDto.Point }, drawDto.Color, drawDto.Size, drawDto.Eraser);
            game.CanvasStrokes.Add(stroke);
        }
        else if (drawDto.Type == DrawType.Move)
        {
            if (game.CanvasStrokes.Count > 0)
            {
                var last = game.CanvasStrokes.Last();
                last.Points.Add(drawDto.Point);
            }
        }

        _gameRepository.Save(game);
    }

    public void ClearCanvasStrokes(string roomId)
    {
        var game = GetGame(roomId);
        game.CanvasStrokes.Clear();
        _gameRepository.Save(game);
    }

    public IReadOnlyList<StrokeDto> GetCanvasStrokes(string roomId)
    {
        var game = GetGame(roomId);
        return game.CanvasStrokes.AsReadOnly();
    }
}