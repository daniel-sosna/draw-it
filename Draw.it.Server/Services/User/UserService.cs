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
            throw new AppException("User name cannot be empty", System.Net.HttpStatusCode.BadRequest);
        }
        var user = new UserModel
        {
            Id = _userRepository.GetNextId(),
            Name = name
        };
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
        return _userRepository.FindById(userId) ?? throw new EntityNotFoundException($"User with id={userId} not found");
    }

    public void SetRoom(long userId, string? roomId)
    {
        var user = GetUser(userId);
        user.RoomId = roomId;
        _userRepository.Save(user);
    }
}