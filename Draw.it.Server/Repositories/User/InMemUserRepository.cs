using System.Collections.Concurrent;
using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public class InMemUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<long, UserModel> _users = new();
    private long _nextId = 0;

    public void Save(UserModel user)
    {
        // Only allow saving users that were created via GetNextId (i.e. their Id must be
        // strictly less than the current next id). This keeps ids sequential and prevents
        // callers from inserting out-of-order ids. The check is done with a volatile read
        // to ensure visibility between threads.
        var currentNext = Volatile.Read(ref _nextId);
        if (user.Id >= currentNext)
            throw new InvalidOperationException(
                "User indexes must be sequential: user.Id must be less than the repository's next id. "
                + "Use GetNextId to get a new id before creating a user.");

        _users[user.Id] = user;
    }

    public bool DeleteById(long id)
    {
        return _users.TryRemove(id, out _);
    }

    public UserModel? FindById(long id)
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
        return Interlocked.Increment(ref _nextId) - 1;  // i.e. return current value, then increment.
    }

    public IEnumerable<UserModel> FindByRoomId(string roomId)
    {
        return _users.Values.Where(u => u.RoomId == roomId);
    }

    public UserModel FindAiPlayerByRoomId(string roomId)
    {
        return _users.Values.First(u => u.RoomId == roomId && u.IsAi);
    }
}