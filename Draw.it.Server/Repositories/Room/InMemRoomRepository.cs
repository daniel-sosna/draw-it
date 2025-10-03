using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Repositories.Room;

public class InMemRoomRepository : IRoomRepository
{
    private static readonly Dictionary<string, RoomModel> ActiveRooms = new Dictionary<string, RoomModel>();
    private readonly object _lock = new object();

    public RoomModel Save(RoomModel room)
    {
        lock (_lock)
        {
            ActiveRooms.Add(room.Id, room);
            return room;
        }
    }

    public bool ExistsById(string id)
    {
        lock (_lock)
        {
            return ActiveRooms.ContainsKey(id);
        }
    }
}