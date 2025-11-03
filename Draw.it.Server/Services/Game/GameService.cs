using System.Net;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Game;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Enums;
using Draw.it.Server.Repositories.WordPool;
using Draw.it.Server.Services.WordPool;


namespace Draw.it.Server.Services.Game;

public class GameService : IGameService
{
    private readonly ILogger<GameService> _logger;
    private readonly IGameRepository _gameRepository;
    private readonly IRoomService _roomService;
    private readonly IWordPoolService _wordPoolService; // <-- Pakeista iš Repository į Service
    private readonly Random _random = new();

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

        var randomWord = _wordPoolService.GetRandomWordByCategoryId(room.Settings.CategoryId);
        string firstWord = randomWord.ToString();

        var gameSession = new GameModel
        {
            RoomId = roomId,
            CurrentRound = 1,
            CurrentDrawerId = turnOrderIds[0],
            WordToDraw = firstWord
        };

        _gameRepository.Save(gameSession);
        _logger.LogInformation("Game session for room id={roomId} created. First drawer: {drawerId}, Word: {word}", roomId, gameSession.CurrentDrawerId, firstWord);
    }

    public long GetDrawerId(string roomId)
    {
        return GetGame(roomId).CurrentDrawerId;
    }
    
    public void SetDrawerId(GameModel session, long newDrawerId)
    {
        session.CurrentDrawerId = newDrawerId;
        _logger.LogInformation("Room {roomId}: Drawer ID manually set to {drawerId}", session.RoomId, newDrawerId);
    }
}