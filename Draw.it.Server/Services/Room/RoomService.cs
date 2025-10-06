using System.Net;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.Session;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Services.Session;

namespace Draw.it.Server.Services.Room;

public class RoomService : IRoomService
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly ILogger<RoomService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly ISessionService _sessionService;

    public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepository, ISessionService sessionService)
    {
        _logger = logger;
        _roomRepository = roomRepository;
        _sessionService = sessionService;
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

    public RoomModel CreateRoom(SessionModel session)
    {
        if (session.RoomId != null)
        {
            throw new AppException("You are already in a room. Leave the current room before creating a new one.", HttpStatusCode.Conflict);
        }
        var roomId = GenerateUniqueRoomId();
        var room = new RoomModel
        {
            Id = roomId,
            HostId = session.UserId,
            PlayerIds = new List<long> { session.UserId }
        };
        _roomRepository.Save(room);
        _logger.LogInformation("Room with id={roomId} created", roomId);
        _sessionService.SetRoom(session.Id, roomId);
        return room;
    }

    // Placeholder!
    public void DeleteRoom(string roomId, SessionModel session)
    {
        var room = GetRoom(roomId);
        if (room.HostId != session.UserId)
        {
            throw new AppException("Only the host can delete the room.", HttpStatusCode.Forbidden);
        }
        // TODO: Check if game is in progress

        // TODO: Remove roomId from all users that had this roomId set
    }

    public RoomModel GetRoom(string roomId)
    {
        return _roomRepository.FindById(roomId) ?? throw new EntityNotFoundException($"Room with id={roomId} not found");
    }

    public void JoinRoom(string roomId, SessionModel session)
    {
        if (session.RoomId != null)
        {
            throw new AppException($"You are already in the room with id={session.RoomId}. Leave the current room before joining another one.", HttpStatusCode.Conflict);
        }
        var room = GetRoom(roomId);
        // TODO: Check if game is in progress
        // TODO: Check on number of players
        room.PlayerIds.Add(session.UserId);
        _roomRepository.Save(room);
        _sessionService.SetRoom(session.Id, roomId);
    }

    public void LeaveRoom(string roomId, SessionModel session)
    {
        if (session.RoomId != roomId)
        {
            throw new AppException($"You are not in the room with id={roomId}.", HttpStatusCode.Conflict);
        }
        var room = GetRoom(roomId);
        if (room.HostId == session.UserId)
        {
            throw new AppException("Host cannot leave the room. Consider deleting the room instead.", HttpStatusCode.Forbidden);
        }
        // TODO: Check if game is in progress
        room.PlayerIds.Remove(session.UserId);
        _roomRepository.Save(room);
        _sessionService.SetRoom(session.Id, null);
    }
}
