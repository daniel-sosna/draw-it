using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.User;

public interface IUserService
{
    UserModel CreateUser(string name);
    void DeleteUser(long userId);
    UserModel GetUser(long userId);
    void SetRoom(long userId, string? roomId);
}