using Draw.it.Server.Models;
using Draw.it.Server.Repositories.User;

namespace Draw.it.Server.Services.User;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public UserRec CreateUser(string name)
    {
        var userRec = new UserRec{Name = name};
        _logger.LogInformation("User with name={} created", name);
        return _userRepository.Save(userRec);
    }

    public UserRec? FindUserById(long id)
    {
        return _userRepository.FindById(id);
    }
}