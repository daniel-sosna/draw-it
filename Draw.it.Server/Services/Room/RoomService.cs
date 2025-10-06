using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Repositories.Room;

namespace Draw.it.Server.Services.Room
{
    public class RoomService : IRoomService
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly ILogger<RoomService> _logger;
        private readonly IRoomRepository _roomRepository;

        public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository)
        {
            _logger = logger;
            _roomRepository = roomRepository;
        }

        private string GenerateRandomRoomId()
        {
            var random = new Random();

            return new string(Enumerable.Repeat(Chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateUniqueRoomId()
        {
            string roomId;

            do
            {
                roomId = GenerateRandomRoomId();
            } while (_roomRepository.ExistsById(roomId));

            return roomId;
        }

        public RoomModel CreateRoom(long hostId)
        {
            var roomId = GenerateUniqueRoomId();
            var room = new RoomModel
            {
                Id = roomId,
                HostId = hostId,
                PlayerIds = new List<long> { hostId }
            };
            _roomRepository.Save(room);
            _logger.LogInformation("Room with id={roomId} created", roomId);
            return room;
        }

        public void DeleteRoom(string roomId)
        {
            if (!_roomRepository.DeleteById(roomId))
            {
                throw new EntityNotFoundException($"Room with id={roomId} not found");
            }
        }

        public RoomModel GetRoom(string roomId)
        {
            return _roomRepository.GetById(roomId) ?? throw new EntityNotFoundException($"Room with id={roomId} not found");
        }

        public void JoinRoom(string roomId, long userId)
        {
            var room = _roomRepository.GetById(roomId);
            if (room == null)
            {
                throw new EntityNotFoundException($"Room with id={roomId} not found");
            }
            if (room.PlayerIds.Contains(userId))
            {
                throw new InvalidOperationException($"User with id={userId} is already in the room with id={roomId}");
            }
            room.PlayerIds.Add(userId);
            _roomRepository.Save(room);
        }

        public void LeaveRoom(string roomId, long userId)
        {
            var room = _roomRepository.GetById(roomId);
            if (room == null)
            {
                throw new EntityNotFoundException($"Room with id={roomId} not found");
            }
            if (!room.PlayerIds.Contains(userId))
            {
                throw new InvalidOperationException($"User with id={userId} is not in the room with id={roomId}");
            }
            if (room.HostId == userId)
            {
                throw new InvalidOperationException("Host cannot leave the room. Consider deleting the room instead.");
            }
            room.PlayerIds.Remove(userId);
            _roomRepository.Save(room);
        }
    }
}
