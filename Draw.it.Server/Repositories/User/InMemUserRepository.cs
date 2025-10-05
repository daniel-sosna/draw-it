using System.Collections.Concurrent;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public class InMemUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<long, UserModel> _users = new();

    public void Save(UserModel user)
    {
        _users[user.Id] = user;
    }

    public void Delete(UserModel user)
    {
        _users.TryRemove(user.Id, out _);
    }

    public UserModel? GetById(long id)
    {
        if (!_users.TryGetValue(id, out var user))
        {
            throw new EntityNotFoundException($"User with id {id} not found.");
        }
        return user;
    }

    public IEnumerable<UserModel> GetAll()
    {
        return _users.Values;
    }
}