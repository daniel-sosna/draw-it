using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Draw.it.Server.Controllers
{
    [ApiController]
    [Route("drawitem/[controller]")]
    public class RoomsController : ControllerBase
    {
        private static readonly Dictionary<string, Room> ActiveRooms = new Dictionary<string, Room>();
        private static readonly Random random = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Helper function to generate a random alphanumeric ID
        private string GenerateRandomRoomId()
        {
            return new string(Enumerable.Repeat(Chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpPost(Name = "PostRoom")]
        public IActionResult CreateRoom([FromBody] RoomSettings settings)
        {
            string roomId;
            lock (ActiveRooms)
            {
                do
                {
                    roomId = GenerateRandomRoomId();
                } while (ActiveRooms.ContainsKey(roomId));

                var newRoom = new Room
                {
                    Id = roomId,
                    Settings = settings,
                    Players = new List<string>()
                };
                ActiveRooms.Add(roomId, newRoom);
            }

            return Ok(new { roomId });
        }
    }
    
    // Model for the room itself
    public class Room
    {
        public string Id { get; set; }
        public RoomSettings Settings { get; set; }
        public List<string> Players { get; set; }
    }
    
    // Model to receive data from the frontend
    public class RoomSettings
    {
        public string RoomName { get; set; }
        public string[] Categories { get; set; }
        public string[] CustomWords { get; set; }
        public int DrawingTime { get; set; }
        public int NumberOfRounds { get; set; }
    }
}