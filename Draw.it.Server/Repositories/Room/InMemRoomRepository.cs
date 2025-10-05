using System.Collections.Concurrent;
using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Repositories.Room;

public class InMemRoomRepository : IRoomRepository
{
    private readonly ConcurrentDictionary<string, RoomModel> _rooms = new();

    public void Save(RoomModel room)
    {
        _rooms[room.Id] = room;
    }

    public bool DeleteById(string id)
    {
        return _rooms.TryRemove(id, out _);
    }

    public RoomModel? GetById(string id)
    {
        _rooms.TryGetValue(id, out var room);
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