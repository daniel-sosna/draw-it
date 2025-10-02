using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        string GenerateUniqueRoomId();

        void CreateAndAddRoom(string roomId, RoomSettingsModel settings);
    }
}
