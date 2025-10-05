using System.Collections.Concurrent;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public class InMemUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<long, UserModel> _users = new();

    public void Save(UserModel user)
    {
        _users[user.Id] = user;
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
}