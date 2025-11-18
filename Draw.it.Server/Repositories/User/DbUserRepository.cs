using Draw.it.Server.Data;
using Draw.it.Server.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Draw.it.Server.Repositories.User;

public class DbUserRepository(ApplicationDbContext context) : IUserRepository
{
    public void Save(UserModel entity)
    {
        if (entity.Id == 0)
        {
            context.Users.Add(entity);
        }
        else
        {
            var existing = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == entity.Id);
            if (existing is null) context.Users.Add(entity);
            else context.Users.Update(entity);
        }
        context.SaveChanges();
    }

    public bool DeleteById(long id)
    {
        var entity = context.Users.Find(id);
        if (entity is null) return false;
        context.Users.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public UserModel? FindById(long id)
    {
        return context.Users.Find(id);
    }

    public IEnumerable<UserModel> GetAll()
    {
        return context.Users.AsNoTracking().ToList();
    }

    public long GetNextId()
    {
        // Simple strategy: max(id)+1 (sufficient for dev/local)
        var currentMax = context.Users.AsNoTracking().Select(u => (long?)u.Id).Max() ?? 0;
        return currentMax + 1;
    }

    public IEnumerable<UserModel> FindByRoomId(string roomId)
    {
        return context.Users.AsNoTracking().Where(u => u.RoomId == roomId).ToList();
    }
}


