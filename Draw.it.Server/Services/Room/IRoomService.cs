using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        string GenerateUniqueRoomId();
        void CreateRoom(string roomId, RoomSettingsModel settings);
        bool JoinRoom(string roomId, UserModel user);
        void DeleteRoom(string roomId);
        RoomModel GetRoom(string roomId);
    }
}
