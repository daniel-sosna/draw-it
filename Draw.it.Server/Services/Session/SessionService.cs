using Draw.it.Server.Exceptions;
using Draw.it.Server.Repositories.Session;
using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Session;

public class SessionService : ISessionService
{
    private readonly ILogger<SessionService> _logger;
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ILogger<SessionService> logger, ISessionRepository sessionRepository)
    {
        _logger = logger;
        _sessionRepository = sessionRepository;
    }

    public SessionModel CreateSession(long userId)
    {
        var session = new SessionModel { UserId = userId };
        _sessionRepository.Save(session);
        _logger.LogInformation("Created session {SessionId} for user with id={UserId}", session.Id, userId);
        return session;
    }

    public void DeleteSession(string sessionId)
    {
        if (!_sessionRepository.DeleteById(sessionId))
        {
            throw new EntityNotFoundException($"Session with id={sessionId} not found");
        }
    }

    public SessionModel GetSession(string sessionId)
    {
        return _sessionRepository.FindById(sessionId) ?? throw new EntityNotFoundException($"Session with id={sessionId} not found");
    }

    public void SetRoom(string sessionId, string roomId)
    {
        var session = GetSession(sessionId);
        session.RoomId = roomId;
        _sessionRepository.Save(session);
    }
}
