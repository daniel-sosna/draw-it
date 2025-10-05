using System.Collections.Concurrent;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Repositories.Room;

public class InMemRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<string, RoomModel> _rooms = new();

    public void Save(RoomModel room)
    {
        _rooms[room.Id] = room;
    }

    public void Delete(RoomModel room)
    {
        _rooms.TryRemove(room.Id, out _);
    }

    public RoomModel GetById(string id)
    {
        if (!_rooms.TryGetValue(id, out var room))
        {
            throw new EntityNotFoundException($"Room with id '{id}' not found.");
        }
        return room;
    }

    public IEnumerable<RoomModel> GetAll()
    {
        return _rooms.Values;
    }

    public bool ExistsById(string id)
    {
        return _rooms.ContainsKey(id);
    }
}