using Draw.it.Server.Models.User;

namespace Draw.it.Server.Repositories.User;

public interface IUserRepository
{
    UserModel Save(UserModel user);

    UserModel? FindById(long id);
}