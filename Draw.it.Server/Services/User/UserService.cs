namespace Draw.it.Server.Services.User;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private long _idSequence;
    private readonly List<long> _activeIds;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
        _activeIds = [];
    }

    public long GenerateUserId()
    {
        _logger.LogInformation("Generating user id: {}", _idSequence);
        _activeIds.Add(_idSequence);
        return _idSequence++;
    }

    public List<long> GetActiveUserIds()
    {
        return _activeIds;
    }
}