using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.User;
using Draw.it.Server.Services.User;
using Microsoft.Extensions.Logging;
using Moq;

namespace draw_it.Tests.Services;

public class UserServiceTest
{
    private const string NAME = "TEST_NAME"; 
    
    private IUserService? _userService;
    private Mock<IUserRepository>? _userRepository;
    private Mock<ILogger<UserService>>? _logger;
    
    [SetUp]
    public void Setup()
    {
        _userRepository = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_logger.Object, _userRepository.Object);
    }

    [Test]
    public void whenCreateUser_thenUserCreatedSuccessfully()
    {
        UserModel user = _userService.CreateUser(NAME);
        
        _userRepository.Verify(r => r.GetNextId(), Times.Once);
        _userRepository.Verify(r => r.Save(It.IsAny<UserModel>()), Times.Once);
        
        Assert.That(NAME, Is.EqualTo(user.Name));
    }
}