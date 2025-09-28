using Draw.it.Server.Controllers.Rooms;
using Draw.it.Server.Models;
using System.Collections.Generic;
using System.Linq;

namespace Draw.it.Server.Services.Rooms
{
    public class RoomService : IRoomService
    {
        private static readonly Dictionary<string, Room> ActiveRooms = new Dictionary<string, Room>();
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

        public void CreateAndAddRoom(string roomId, RoomSettings settings)
        {
            var newRoom = new Room
            {
                Id = roomId,
                Settings = settings,
                Players = new List<string>()
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
