using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Repositories.Room;

public interface IRoomRepository
{
    RoomModel Save(RoomModel room);

    bool ExistsById(string id);
    
    RoomModel? FindById(string id); 
}