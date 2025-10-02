using Draw.it.Server.Models;

namespace Draw.it.Server.Repositories.User;

public interface IUserRepository
{
    UserRec Save(UserRec user);

    UserRec? FindById(long id);
}