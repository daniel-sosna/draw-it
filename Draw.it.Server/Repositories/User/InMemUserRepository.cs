using Draw.it.Server.Exceptions;
using Draw.it.Server.Models;

namespace Draw.it.Server.Repositories.User;

public class InMemUserRepository : IUserRepository
{
    private long _idSequence;
    private readonly List<UserRec> _users;
    private readonly object _lock = new object();

    public InMemUserRepository()
    {
        _users = [];
    }
    
    public UserRec Save(UserRec user)
    {
        lock (_lock)
        {
            // User id exists => update user
            if (user.Id != 0)
            {
                Update(user);
            }
        
            // If no user id create user
            var newId = ++_idSequence;
            user.Id = newId;
            _users.Add(user);
            return user;
        }
       
    }

    public UserRec? FindById(long id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
    }

    private UserRec Update(UserRec user)
    {
        lock (_lock)
        {
            var idx = _users.FindIndex(u => u.Id == user.Id);
            if (idx == -1) throw new EntityNotFoundException($"User {user.Id} not found");
            _users[idx] = user;  
            return user;
        }
    }
}