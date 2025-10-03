using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Session;

public interface ISessionService
{
    SessionModel? Get(string sessionId);
    void Add(SessionModel session);
    bool Remove(string sessionId);
}
