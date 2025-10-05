using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.Room;

public class InMemRoomRepository : IRoomRepository
{
    private static readonly Dictionary<string, RoomModel> ActiveRooms = new Dictionary<string, RoomModel>();
    private readonly object _lock = new object();

    public RoomModel Save(RoomModel room)
    {
        lock (_lock)
        {
            ActiveRooms[room.Id] = room;
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

    public RoomModel? FindById(string id)
    {
        lock (_lock)
        {
            ActiveRooms.TryGetValue(id, out var room);
            return room;
        }
    }
    
    public void AddUserToRoom(string roomId, UserModel user)
    {
        lock (_lock) 
        {
            if (ActiveRooms.TryGetValue(roomId, out var room))
            {
                room.Players.Add(user);
            }
        }
    }
}
