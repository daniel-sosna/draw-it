using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Controllers.Room.DTO;

namespace Draw.it.Server.Services.Room
{
    public interface IRoomService
    {
        string GenerateUniqueRoomId();
        RoomModel GetRoom(string roomId);
        RoomModel AddPlayerToRoom(string roomId, UserModel user, bool isHost);
        RoomModel SetPlayerReady(string roomId, long userId, bool isReady);
        void UpdateRoomSettings(string roomId, PatchRoomSettingsDto settingsPatch);
        RoomModel StartGame(string roomId);
        void JoinRoom(string roomId, UserModel user);
    }
}