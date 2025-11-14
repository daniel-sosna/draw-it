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

    public GameModel GetGame(string roomId)
    {
        return _gameRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Game for room id={roomId} not found");
    }

    public void DeleteGame(string roomId)
    {
        if (!_gameRepository.DeleteById(roomId))
        {
            _logger.LogWarning("Attempted to delete non-existent game session for room id={roomId}", roomId);
        }

        _gameRepository.DeleteById(roomId);
    }

    public void CreateGame(string roomId)
    {
        var room = _roomService.GetRoom(roomId);
        var players = _roomService.GetUsersInRoom(roomId).ToList();

        if (room.Status != RoomStatus.InGame)
        {
            throw new AppException($"Cannot start game session: Room {roomId} status is invalid.", HttpStatusCode.Conflict);
        }

        var turnOrderIds = players.Select(p => p.Id).ToList();

        var gameSession = new GameModel
        {
            RoomId = roomId,
            CurrentRound = 1,
            CurrentTurnIndex = 0,
            CurrentDrawerId = turnOrderIds[0],
            WordToDraw = GetRandomWord(room.Settings.CategoryId)
        };

        _gameRepository.Save(gameSession);
        _logger.LogInformation("Game session for room id={roomId} created. First drawer: {drawerId}, Word: {word}", roomId, gameSession.CurrentDrawerId, gameSession.WordToDraw);
    }

    public string GetRandomWord(long categoryId)
    {
        var randomWord = _wordPoolService.GetRandomWordByCategoryId(categoryId);
        return randomWord.Value;
    }

    public long GetDrawerId(string roomId)
    {
        return GetGame(roomId).CurrentDrawerId;
    }

    public void SetDrawerId(string roomId, long newDrawerId)
    {
        var session = GetGame(roomId);

        session.CurrentDrawerId = newDrawerId;

        _gameRepository.Save(session);
        _logger.LogInformation("Room {roomId}: Drawer ID manually set to {drawerId}", session.RoomId, newDrawerId);
    }

    private long GetNextDrawerId(GameModel session)
    {
        int totalRounds = _roomService.GetRoom(session.RoomId).Settings.NumberOfRounds;

        var turnOrderIds = _roomService.GetUsersInRoom(session.RoomId).Select(p => p.Id).ToList();

        int nextTurnIndex = (session.CurrentTurnIndex + 1) % turnOrderIds.Count;

        if (nextTurnIndex == 0)
        {
            int newRoundValue = session.CurrentRound + 1;

            if (newRoundValue > totalRounds)
            {
                return -1;
            }

            session.CurrentRound = newRoundValue;
        }

        session.CurrentTurnIndex = nextTurnIndex;

        _gameRepository.Save(session);

        return turnOrderIds[session.CurrentTurnIndex];

    }


    public bool AddGuessedPlayer(string roomId, long userId)
    {
        var session = GetGame(roomId);

        if (session.GuessedPlayersIds.Contains(userId))
        {
            return false;
        }

        session.GuessedPlayersIds.Add(userId);

        _gameRepository.Save(session);

        var allPlayersCount = _roomService.GetUsersInRoom(roomId).Count();
        var requiredGuessers = allPlayersCount - 1;

        return session.GuessedPlayersIds.Count >= requiredGuessers;
    }

    public void ClearGuessedPlayers(string roomId)
    {
        var session = GetGame(roomId);

        session.GuessedPlayersIds.Clear();

        _gameRepository.Save(session);

        _logger.LogInformation("Room {roomId}: Guessed players list cleared.", roomId);
    }


    public bool AdvanceTurn(string roomId)
    {
        var session = GetGame(roomId);
        var room = _roomService.GetRoom(roomId);

        long nextDrawerId = GetNextDrawerId(session);

        if (nextDrawerId == -1)
        {
            _gameRepository.Save(session);
            return true;
        }

        session.CurrentDrawerId = nextDrawerId;
        session.WordToDraw = GetRandomWord(room.Settings.CategoryId);
        ClearGuessedPlayers(roomId);

        _gameRepository.Save(session);

        return false;
    }

    public string GetMaskedWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return string.Empty;

        return new string(word.Select(c => char.IsWhiteSpace(c) ? ' ' : '*').ToArray());
    }
}