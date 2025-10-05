using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.User;
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

    public UserModel CreateUser(string name)
    {
        var userRec = new UserModel { Name = name };
        _logger.LogInformation("User with name={} created", name);
        _userRepository.Save(userRec);
        return userRec;
    }

    public UserModel GetUserById(long id)
    {
        return _userRepository.GetById(id) ?? throw new EntityNotFoundException($"User with id={id} not found");
    }
}