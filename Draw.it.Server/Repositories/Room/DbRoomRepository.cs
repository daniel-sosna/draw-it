using Draw.it.Server.Data;
using Draw.it.Server.Models.Room;
using Microsoft.EntityFrameworkCore;

namespace Draw.it.Server.Repositories.Room;

public class DbRoomRepository(ApplicationDbContext context) : IRoomRepository
{
    public void Save(RoomModel entity)
    {
        var tracked = context.Rooms.Local.FirstOrDefault(r => r.Id == entity.Id);
        if (tracked is not null)
        {
            context.Entry(tracked).CurrentValues.SetValues(entity);
        }
        else
        {
            var existingTrackedEntity = context.Rooms.Find(entity.Id);
            if (existingTrackedEntity is null)
            {
                context.Rooms.Add(entity);
            }
            else
            {
                context.Entry(existingTrackedEntity).CurrentValues.SetValues(entity);
            }
        }
        context.SaveChanges();
    }

    public bool DeleteById(string id)
    {
        var entity = context.Rooms.Find(id);
        if (entity is null) return false;
        context.Rooms.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public RoomModel? FindById(string id)
    {
        return context.Rooms.Find(id);
    }

    public IEnumerable<RoomModel> GetAll()
    {
        return context.Rooms.AsNoTracking().ToList();
    }

    public bool ExistsById(string id)
    {
        return context.Rooms.AsNoTracking().Any(r => r.Id == id);
    }
}


