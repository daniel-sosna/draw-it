namespace Draw.it.Server.Services.User;

public interface IUserService
{
    long GenerateUserId();

    List<long> GetActiveUserIds();
}