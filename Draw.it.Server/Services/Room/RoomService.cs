using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.Room
{
    public class RoomService : IRoomService
    {
        private static readonly Dictionary<string, RoomModel> ActiveRooms = new Dictionary<string, RoomModel>();
        private static readonly object ActiveRoomsLock = new object();

        private static readonly Random random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private string GenerateRandomRoomId()
        {
            return new string(Enumerable.Repeat(Chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerateUniqueRoomId()
        {
            string roomId;
            lock (ActiveRoomsLock)
            {
                do
                {
                    roomId = GenerateRandomRoomId();
                } while (ActiveRooms.ContainsKey(roomId));
            }
            return roomId;
        }

        public void CreateAndAddRoom(string roomId, RoomSettingsModel settings)
        {
            var newRoom = new RoomModel
            {
                Id = roomId,
                Settings = settings,
                Players = new List<UserModel>()
            };

            lock (ActiveRoomsLock)
            {
                if (!ActiveRooms.ContainsKey(roomId))
                {
                    ActiveRooms.Add(roomId, newRoom);
                }
            }
        }
    }
}
