using System.Data;
using Draw.it.Server.Data;
using Draw.it.Server.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Draw.it.Server.Repositories.User;

public class DbUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DbUserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Save(UserModel entity)
    {
        var exists = _dbContext.Users.AsNoTracking().Any(u => u.Id == entity.Id);
        if (exists)
            _dbContext.Users.Update(entity);
        else
            _dbContext.Users.Add(entity);

        _dbContext.SaveChanges();
    }

    public bool DeleteById(long id)
    {
        var entity = _dbContext.Users.Find(id);
        if (entity is null) return false;
        _dbContext.Users.Remove(entity);
        _dbContext.SaveChanges();
        return true;
    }

    public UserModel? FindById(long id)
    {
        return _dbContext.Users.Find(id);
    }

    public IEnumerable<UserModel> GetAll()
    {
        return _dbContext.Users.AsNoTracking().ToList();
    }

    public long GetNextId()
    {
        // Fetch next value from global sequence created in OnModelCreating
        using var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT nextval('user_id_seq')";
        var result = command.ExecuteScalar();
        return Convert.ToInt64(result);
    }

    public IEnumerable<UserModel> FindByRoomId(string roomId)
    {
        return _dbContext.Users.AsNoTracking().Where(u => u.RoomId == roomId).ToList();
    }
}


