using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        RoomModel CreateRoom(UserModel user);
        void DeleteRoom(string roomId, UserModel user);
        RoomModel GetRoom(string roomId);
        void JoinRoom(string roomId, UserModel user);
        void LeaveRoom(string roomId, UserModel user);
        IEnumerable<UserModel> GetUsersInRoom(string roomId);
        Task SetSettingsAsync(string roomId, string categoryId, string drawingTime, string numberOfRounds);
        void StartGame(string roomId, UserModel user);
        void UpdateSettings(string roomId, UserModel user, RoomSettingsModel newSettings);
    }
}
