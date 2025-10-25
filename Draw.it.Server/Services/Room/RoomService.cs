using System.Net;
using Draw.it.Server.Enums;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Repositories.User;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Services.Room;

public class RoomService : IRoomService
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly ILogger<RoomService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository, IUserService userService, IUserRepository userRepository)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _userService = userService;
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

    /// <summary>
    /// Create a new room and assign user as host
    /// </summary>
    public RoomModel CreateRoom(UserModel user)
    {
        if (user.RoomId != null)
        {
            throw new AppException("You are already in a room. Leave the current room before creating a new one.", HttpStatusCode.Conflict);
        }

        var roomId = GenerateUniqueRoomId();
        var room = new RoomModel
        {
            Id = roomId,
            HostId = user.Id
        };

        _roomRepository.Save(room);
        _logger.LogInformation("Room with id={roomId} created", roomId);

        _userService.SetRoom(user.Id, roomId);
        _userService.SetReadyStatus(user.Id, true);

        return room;
    }

    /// <summary>
    /// Delete room (host only)
    /// </summary>
    public void DeleteRoom(string roomId, UserModel user)
    {
        if (user.RoomId != roomId)
        {
            throw new AppException($"You are not in the room with id={roomId}.", HttpStatusCode.Conflict);
        }

        var room = GetRoom(roomId);
        if (room.HostId != user.Id)
        {
            throw new AppException("Only the host can delete the room.", HttpStatusCode.Forbidden);
        }
        if (room.Status == RoomStatus.InGame)
        {
            throw new AppException("Cannot delete room while the game is in progress.", HttpStatusCode.Conflict);
        }

        _userService.RemoveRoomFromAllUsers(roomId);

        _roomRepository.DeleteById(roomId);
    }

    /// <summary>
    /// Get room by id
    /// </summary>
    public RoomModel GetRoom(string roomId)
    {
        return _roomRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Room with id={roomId} not found");
    }

    /// <summary>
    /// Get room settings
    /// </summary>
    public RoomSettingsModel GetRoomSettings(string roomId)
    {
        var room = GetRoom(roomId);
        return room.Settings;
    }

    /// <summary>
    /// Get all players in a room
    /// </summary>
    public IEnumerable<UserModel> GetUsersInRoom(string roomId)
    {
        if (!_roomRepository.ExistsById(roomId))
        {
            throw new EntityNotFoundException($"Room with id={roomId} not found");
        }

        return _userRepository.FindByRoomId(roomId);
    }

    /// <summary>
    /// Assign a player to an existing room
    /// </summary>
    public void JoinRoom(string roomId, UserModel user)
    {

        if (user.RoomId != null)
        {
            throw new AppException($"You are already in the room with id={user.RoomId}. Leave the current room before joining another one.", HttpStatusCode.Conflict);
        }

        var players = GetUsersInRoom(roomId).ToList();
        if (players.Any(p => p.Name == user.Name))
        {
            throw new AppException($"User with username {user.Name} is already in the room. Please create other username.", HttpStatusCode.Conflict);
        }

        var room = GetRoom(roomId);
        if (room.Status != RoomStatus.InLobby)
        {
            throw new AppException("Cannot join room: Game is already in progress or has ended.", HttpStatusCode.Conflict);
        }
        // TODO: Check on number of players

        _userService.SetRoom(user.Id, roomId);
        _userService.SetReadyStatus(user.Id, false);
    }

    /// <summary>
    /// Remove a player from a room
    /// </summary>
    public void LeaveRoom(string roomId, UserModel user)
    {
        if (user.RoomId != roomId)
        {
            throw new AppException($"You are not in the room with id={roomId}.", HttpStatusCode.Conflict);
        }

        var room = GetRoom(roomId);
        if (room.HostId == user.Id)
        {
            throw new AppException("Host cannot leave the room. Consider deleting the room instead.", HttpStatusCode.Forbidden);
        }
        if (room.Status == RoomStatus.InGame)
        {
            throw new AppException("Cannot leave room while the game is in progress.", HttpStatusCode.Conflict);
        }

        _userService.SetRoom(user.Id, null);
    }

    /// <summary>
    /// Check if user is host of a room
    /// </summary>
    public bool IsHost(string roomId, UserModel user)
    {
        if (user.RoomId != roomId)
        {
            throw new AppException($"You are not in the room with id={roomId}.", HttpStatusCode.Conflict);
        }

        var room = GetRoom(roomId);

        return room.HostId == user.Id;
    }

    /// <summary>
    /// Start a game for a room (host only)
    /// </summary>
    public void StartGame(string roomId, UserModel user)
    {
        var room = GetRoom(roomId);
        if (room.HostId != user.Id)
        {
            throw new AppException("Only the host can start the game.", HttpStatusCode.Forbidden);
        }
        if (room.Status != RoomStatus.InLobby)
        {
            throw new AppException("Cannot start game: It is already in progress or has ended.", HttpStatusCode.Conflict);
        }

        var players = GetUsersInRoom(roomId).ToList();
        if (players.Count < 2)
        {
            throw new AppException("Cannot start game: At least 2 players are required.", HttpStatusCode.Conflict);
        }

        var notReadyPlayers = players.Where(p => !p.IsReady).ToList();
        if (notReadyPlayers.Any())
        {
            var notReadyNames = string.Join(", ", notReadyPlayers.Select(p => p.Name));
            throw new AppException($"Cannot start game. The following players are not ready: {notReadyNames}.", HttpStatusCode.Conflict);
        }

        room.Status = RoomStatus.InGame;

        _roomRepository.Save(room);
    }

    /// <summary>
    /// Update room settings (host only)
    /// </summary>
    public bool UpdateSettings(string roomId, UserModel user, RoomSettingsModel settings)
    {
        var room = GetRoom(roomId);

        // Skip if settings the same
        if (room.Settings.Equals(settings))
        {
            return false;
        }

        if (room.HostId != user.Id)
        {
            throw new AppException("Only the host can update the room.", HttpStatusCode.Forbidden);
        }

        if (room.Status != RoomStatus.InLobby)
        {
            throw new AppException("Cannot change settings: Game is already in progress or has ended.", HttpStatusCode.Conflict);
        }

        room.Settings = settings;
        _roomRepository.Save(room);
        return true;
    }
}
