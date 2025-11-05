using System.Collections.Concurrent;
using Draw.it.Server.Models.Game;

namespace Draw.it.Server.Repositories.Game;

public class InMemGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, GameModel> _gameSessions = new();

    public void Save(GameModel gameSession)
    {
        _gameSessions[gameSession.RoomId] = gameSession;
    }

    public bool DeleteById(string id)
    {
        return _gameSessions.TryRemove(id, out _);
    }

    public GameModel? FindById(string id)
    {
        _gameSessions.TryGetValue(id, out var gameSession);
        return gameSession;
    }

    public IEnumerable<GameModel> GetAll()
    {
        return _gameSessions.Values;
    }
}