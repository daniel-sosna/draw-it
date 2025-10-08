using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Repositories.Room;

public interface IRoomRepository : IRepository<RoomModel, string>
{
    bool ExistsById(string id);
}