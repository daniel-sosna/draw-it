using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.User;

public interface IUserService
{
    UserModel CreateUser(string name);

    UserModel FindUserById(long id);
}