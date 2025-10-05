using System.Collections.Concurrent;
using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Repositories.Session;

public class InMemSessionRepository : ISessionRepository
{
    private readonly ConcurrentDictionary<string, SessionModel> _sessions = new();

    public void Save(SessionModel session)
    {
        _sessions[session.Id] = session;
    }

    public bool DeleteById(string id)
    {
        return _sessions.TryRemove(id, out _);
    }

    public SessionModel? GetById(string id)
    {
        _sessions.TryGetValue(id, out var session);
        return session;
    }

    public IEnumerable<SessionModel> GetAll()
    {
        return _sessions.Values;
    }
}