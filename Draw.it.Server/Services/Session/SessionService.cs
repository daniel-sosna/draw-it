using System.Collections.Concurrent;
using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Session;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly ConcurrentDictionary<string, SessionModel> _sessions = new();

    public SessionService(ILogger<SessionService> logger)
    {
        _logger = logger;
    }

    public SessionModel? Get(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public void Add(SessionModel session)
    {
        _logger.LogInformation("Adding session {SessionId} (user: '{UserName}')", session.SessionId, session.UserName);
        _sessions[session.SessionId] = session;
    }

    public bool Remove(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}
