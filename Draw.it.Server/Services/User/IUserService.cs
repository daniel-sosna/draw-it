using Draw.it.Server.Models.User;

namespace Draw.it.Server.Services.User;

public interface IUserService
{
    UserModel CreateUser(string name);
    void DeleteUser(long userId);
    UserModel GetUser(long userId);
    void SetRoom(long userId, string? roomId);
    void SetConnectedStatus(long userId, bool isConnected);
    void SetReadyStatus(long userId, bool isReady);
    void RemoveRoomFromAllUsers(string roomId);
    void UpdateName(long userId, string newName);
    void CreateAiUser(string roomId);
    UserModel GetAiUserInRoom(string roomId);
}