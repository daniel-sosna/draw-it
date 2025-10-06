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
        name = name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be empty");
        }
        var user = new UserModel { Name = name };
        _userRepository.Save(user);
        _logger.LogInformation("User with name={name} created", name);
        return user;
    }

    public void DeleteUser(long userId)
    {
        if (!_userRepository.DeleteById(userId))
        {
            throw new EntityNotFoundException($"User with id={userId} not found");
        }
    }

    public UserModel GetUser(long userId)
    {
        return _userRepository.GetById(userId) ?? throw new EntityNotFoundException($"User with id={userId} not found");
    }
}