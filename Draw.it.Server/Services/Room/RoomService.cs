using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
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

        public string GenerateUniqueRoomId()
        {
            string roomId;

            do
            {
                roomId = GenerateRandomRoomId();
            } while (_roomRepository.ExistsById(roomId));

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

            if (_roomRepository.ExistsById(roomId))
            {
                throw new DuplicateEntityException("Room with such ID already exists");
            }

            _roomRepository.Save(newRoom);
            _logger.LogInformation("Room with id={} created", roomId);
        }
    }
}
