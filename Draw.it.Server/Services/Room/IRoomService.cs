using Draw.it.Server.Models;

namespace Draw.it.Server.Services.Rooms
{
    public interface IRoomService
    {
        string GenerateUniqueRoomId();

        void CreateAndAddRoom(string roomId, RoomSettings settings);
    }
}
