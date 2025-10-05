using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.Room;

public interface IRoomRepository
{
    RoomModel Save(RoomModel room);

    bool ExistsById(string id);

    RoomModel? FindById(string id);
    
    void AddUserToRoom(string roomId, UserModel user); 

}