using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        string GenerateUniqueRoomId();
        void CreateRoom(string roomId, RoomSettingsModel settings);
        void DeleteRoom(string roomId);
        RoomModel GetRoom(string roomId);
    }
}
