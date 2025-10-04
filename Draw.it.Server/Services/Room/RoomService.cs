using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Services.Room
{
    public class RoomService : IRoomService
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly ILogger<RoomService> _logger;
        private readonly IRoomRepository _roomRepository;
        private readonly IUserService _userService;

        public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository, IUserService userService)
        {
            _logger = logger;
            _roomRepository = roomRepository;
            _userService = userService;
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

        public RoomModel GetRoom(string roomId)
        {
            return _roomRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Room {roomId} not found");
        }


        public void UpdateRoomSettings(string roomId, RoomSettingsModel settings)
        {
            var room = GetRoom(roomId);
            room.Settings = settings;
            _roomRepository.Save(room);
        }

        public RoomModel AddPlayerToRoom(string roomId, UserModel user, bool isHost) // PATAISYTAS PARAŠAS
        {
            var room = _roomRepository.FindById(roomId);

            if (room == null)
            {
                if (!isHost)
                {
                    throw new EntityNotFoundException($"Room {roomId} not found");
                }

                user.IsHost = true;
                user.IsReady = false;

                room = new RoomModel
                {
                    Id = roomId,
                    Settings = new RoomSettingsModel(),
                    Players = new List<UserModel> { user }
                };
            }
            else
            {
                var existingPlayer = room.Players.FirstOrDefault(p => p.Id == user.Id);

                if (existingPlayer != null)
                {
                    existingPlayer.IsHost = isHost;
                    existingPlayer.Name = user.Name;
                }
                else
                {
                    user.IsHost = isHost;
                    user.IsReady = false;
                    room.Players.Add(user);
                }
            }

            return _roomRepository.Save(room);
        }

        public RoomModel SetPlayerReady(string roomId, long userId, bool isReady)
        {
            var room = GetRoom(roomId);

            var player = room.Players.FirstOrDefault(p => p.Id == userId);

            if (player == null)
            {
                throw new EntityNotFoundException($"Player with id={userId} not found in room {roomId}.");
            }

            if (player.IsHost)
            {
                throw new InvalidOperationException("Host cannot set ready status.");
            }

            player.IsReady = isReady;

            return _roomRepository.Save(room);
        }

        public bool CanStartGame(string roomId)
        {
            var room = GetRoom(roomId);

            if (room.Players.Count < 2)
            {
                return false;
            }

            bool allNonHostsReady = room.Players
                .Where(p => !p.IsHost)
                .All(p => p.IsReady);

            return allNonHostsReady;
        }

        public RoomModel StartGame(string roomId)
        {
            var room = GetRoom(roomId);

            room.Status = "IN_GAME";

            if (!CanStartGame(roomId))
            {
                throw new InvalidOperationException(
                    "Žaidimas negali būti pradėtas: ne visi žaidėjai pasiruošę (Ready) arba per mažai žaidėjų.");
            }

            return _roomRepository.Save(room);
        }

        public void JoinRoom(string roomId, UserModel user)
        {
            var room = _roomRepository.FindById(roomId)
                       ?? throw new EntityNotFoundException($"Room with id={roomId} not found");

            if (room.Players.Any(p => p.Id == user.Id))
            {
                throw new DuplicateEntityException($"User with id={user.Id} already in room {roomId}");
            }

            room.Players.Add(user);
            _roomRepository.Save(room);
            _logger.LogInformation("User {} joined room {}", user.Id, roomId);
        }
    }
}
