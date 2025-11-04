using Draw.it.Server.Data;
using Draw.it.Server.Models.Room;
using Microsoft.EntityFrameworkCore;

namespace Draw.it.Server.Repositories.Room;

public class EfRoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EfRoomRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Save(RoomModel entity)
    {
        var exists = _dbContext.Rooms.AsNoTracking().Any(r => r.Id == entity.Id);
        if (exists)
            _dbContext.Rooms.Update(entity);
        else
            _dbContext.Rooms.Add(entity);

        _dbContext.SaveChanges();
    }

    public bool DeleteById(string id)
    {
        var entity = _dbContext.Rooms.Find(id);
        if (entity is null) return false;
        _dbContext.Rooms.Remove(entity);
        _dbContext.SaveChanges();
        return true;
    }

    public RoomModel? FindById(string id)
    {
        return _dbContext.Rooms.Find(id);
    }

    public IEnumerable<RoomModel> GetAll()
    {
        return _dbContext.Rooms.AsNoTracking().ToList();
    }

    public bool ExistsById(string id)
    {
        return _dbContext.Rooms.AsNoTracking().Any(r => r.Id == id);
    }
}


