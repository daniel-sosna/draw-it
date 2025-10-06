using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.Session;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        RoomModel CreateRoom(SessionModel session);
        void DeleteRoom(string roomId);
        RoomModel GetRoom(string roomId);
        void JoinRoom(string roomId, SessionModel session);
        void LeaveRoom(string roomId, SessionModel session);
    }
}
