using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Session;

public interface ISessionService
{
    SessionModel CreateSession(long userId);
    void DeleteSession(string sessionId);
    SessionModel GetSession(string sessionId);
    void SetRoom(string sessionId, string roomId);
}
