using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public interface IUserRepository : IRepository<UserModel, long>
{
    long GetNextId();
}