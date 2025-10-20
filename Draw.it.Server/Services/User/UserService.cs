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

    /// <summary>
    /// Create a new user
    /// </summary>
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

    /// <summary>
    /// Delete user
    /// </summary>
    public void DeleteUser(long userId)
    {
        if (!_userRepository.DeleteById(userId))
        {
            throw new EntityNotFoundException($"User with id={userId} not found");
        }
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    public UserModel GetUser(long userId)
    {
        return _userRepository.FindById(userId) ?? throw new EntityNotFoundException($"User with id={userId} not found");
    }

    /// <summary>
    /// Set the room for a user
    /// </summary>
    public void SetRoom(long userId, string? roomId)
    {
        var user = GetUser(userId);
        user.RoomId = roomId;
        _userRepository.Save(user);
    }

    /// <summary>
    /// Set the connection status for a user
    /// </summary>
    public void SetConnectedStatus(long userId, bool isConnected)
    {
        var user = GetUser(userId);
        user.IsConnected = isConnected;
        _userRepository.Save(user);
    }

    /// <summary>
    /// Set the ready status for a user
    /// </summary>
    public void SetReadyStatus(long userId, bool isReady)
    {
        var user = GetUser(userId);
        user.IsReady = isReady;
        _userRepository.Save(user);
        _logger.LogInformation("User {} ready status set to {}", userId, isReady);
    }

    /// <summary>
    /// Remove room from all users in this room
    /// </summary>
    public void RemoveRoomFromAllUsers(string roomId)
    {
        var users = _userRepository.FindByRoomId(roomId);

        foreach (var user in users)
        {
            user.RoomId = null;

            _userRepository.Save(user);
        }
    }
}