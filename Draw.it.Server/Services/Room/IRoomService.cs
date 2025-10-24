using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        RoomModel CreateRoom(UserModel user);
        void DeleteRoom(string roomId, UserModel user);
        RoomModel GetRoom(string roomId);
        RoomSettingsModel GetRoomSettings(string roomId);
        IEnumerable<UserModel> GetUsersInRoom(string roomId);
        void JoinRoom(string roomId, UserModel user);
        void LeaveRoom(string roomId, UserModel user);
        bool IsHost(string roomId, UserModel user);
        void StartGame(string roomId, UserModel user);
        bool UpdateSettings(string roomId, UserModel user, RoomSettingsModel newSettings);
    }
}