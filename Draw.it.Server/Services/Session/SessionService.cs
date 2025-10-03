using System.Collections.Concurrent;
using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Session;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, SessionModel> _sessions = new();

    public SessionModel? Get(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public void Add(SessionModel session)
    {
        _sessions[session.SessionId] = session;
    }

    public bool Remove(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}
