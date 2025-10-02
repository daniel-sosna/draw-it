using Draw.it.Server.Models;

namespace Draw.it.Server.Services.User;

public interface IUserService
{
    UserRec CreateUser(string name);

    UserRec FindUserById(long id);
}