using System.Net;
using Draw.it.Server.Enums;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Services.User;

namespace Draw.it.Server.Services.Room;

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

    private string GenerateUniqueRoomId()
    {
        string roomId;

        do
        {
            roomId = GenerateRandomRoomId();
        } while (_roomRepository.ExistsById(roomId));

        return roomId;
    }

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
            HostId = user.Id,
            PlayerIds = new List<long> { user.Id },
            Status = RoomStatus.Lobby
        };
        _roomRepository.Save(room);
        _logger.LogInformation("Room with id={roomId} created", roomId);
        _userService.SetRoom(user.Id, roomId);
        return room;
    }

    // Placeholder!
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
            throw new AppException("Cannot delete room while the game is in progress.", HttpStatusCode.Forbidden);
        }

        // TODO: Remove roomId from all users that had this roomId set
    }

    public RoomModel GetRoom(string roomId)
    {
        return _roomRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Room with id={roomId} not found");
    }

    public void JoinRoom(string roomId, UserModel user)
    {
        if (user.RoomId != null)
        {
            throw new AppException($"You are already in the room with id={user.RoomId}. Leave the current room before joining another one.", HttpStatusCode.Conflict);
        }

        var room = GetRoom(roomId);
        if (room.Status != RoomStatus.Lobby)
        {
            throw new AppException("Cannot join room: Game is already in progress or has ended.", HttpStatusCode.Forbidden);
        }
        // TODO: Check on number of players
        room.PlayerIds.Add(user.Id);
        _roomRepository.Save(room);
        _userService.SetRoom(user.Id, roomId);
    }

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
            throw new AppException("Cannot leave room while the game is in progress.", HttpStatusCode.Forbidden);
        }

        room.PlayerIds.Remove(user.Id);
        _roomRepository.Save(room);
        _userService.SetRoom(user.Id, null);
    }
}
