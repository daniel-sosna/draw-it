using System.Collections.Concurrent;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public class InMemUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<long, UserModel> _users = new();
    private long _nextId = 0;

    public void Save(UserModel user)
    {
        _users[user.Id] = user;
        if (user.Id >= _nextId)
        {
            _nextId = user.Id + 1;
        }
    }

    public bool DeleteById(long id)
    {
        return _users.TryRemove(id, out _);
    }

    public UserModel? GetById(long id)
    {
        _users.TryGetValue(id, out var user);
        return user;
    }

    public IEnumerable<UserModel> GetAll()
    {
        return _users.Values;
    }

    public long GetNextId()
    {
        return _nextId++;
    }
}