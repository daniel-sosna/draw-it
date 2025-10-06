using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        RoomModel CreateRoom(long hostId);
        void DeleteRoom(string roomId);
        RoomModel GetRoom(string roomId);
        void JoinRoom(string roomId, long userId);
        void LeaveRoom(string roomId, long userId);
    }
}
