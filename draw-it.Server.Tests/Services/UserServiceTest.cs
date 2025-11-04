using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.User;
using Draw.it.Server.Services.User;
using Microsoft.Extensions.Logging;
using Moq;

namespace draw_it.Tests.Services;

public class UserServiceTest
{
    private const string Name = "TEST_NAME"; 
    private const long Id = 1;
    private const string RoomId = "TEST_ROOM_ID";
    
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
        var user = _userService.CreateUser(Name);
        
        _userRepository.Verify(r => r.GetNextId(), Times.Once);
        _userRepository.Verify(r => r.Save(It.IsAny<UserModel>()), Times.Once);
        
        Assert.That(Name, Is.EqualTo(user.Name));
    }
    
    [Test]
    public void whenCreateUser_andNameIsEmpty_thenThrowAppException()
    {
        Assert.Throws<AppException>(() => _userService.CreateUser(""));
        
        _userRepository.Verify(r => r.GetNextId(), Times.Never);
        _userRepository.Verify(r => r.Save(It.IsAny<UserModel>()), Times.Never);
    }

    [Test]
    public void whenDeleteUser_thenDeleteSuccessful()
    {
        _userRepository
            .Setup(r => r.DeleteById(Id))
            .Returns(true);
        
        _userService.DeleteUser(Id);
        
        _userRepository.Verify(r => r.DeleteById(It.Is<long>(l => l == Id)), Times.Once);
    }
    
    [Test]
    public void whenDeleteUser_andDeleteNotSuccessful_thenThrowException()
    {
        _userRepository
            .Setup(r => r.DeleteById(Id))
            .Returns(false);
        
        Assert.Throws<EntityNotFoundException>(() => _userService.DeleteUser(Id));
        
        _userRepository.Verify(r => r.DeleteById(It.Is<long>(l => l == Id)), Times.Once);
    }
    
    [Test]
    public void whenGetUser_thenReturnUser()
    {
        var expectedUser = new UserModel { Id = Id, Name = Name };
        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns(expectedUser);
        
        var user = _userService.GetUser(Id);
        
        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
        
        Assert.That(expectedUser, Is.EqualTo(user));
    }
    
    [Test]
    public void whenGetUser_andUserNotFound_thenThrowException()
    {
        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns((UserModel?)null);
        
        Assert.Throws<EntityNotFoundException>(() => _userService.GetUser(Id));
        
        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
    }
    
    [Test]
    public void whenSetRoom_thenRoomIdUpdatedAndSaved()
    {
        var user = new UserModel { Id = Id, Name = Name };
        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns(user);

        _userService.SetRoom(Id, RoomId);

        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
        _userRepository.Verify(
            r => r.Save(It.Is<UserModel>(u => u.Id == Id && u.RoomId == RoomId)),
            Times.Once);
    }

    [Test]
    public void whenSetConnectedStatus_thenConnectedStatusUpdatedAndSaved()
    {
        var user = new UserModel { Id = Id, Name = Name };
        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns(user);

        _userService.SetConnectedStatus(Id, true);

        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
        _userRepository.Verify(
            r => r.Save(It.Is<UserModel>(u => u.Id == Id && u.IsConnected)),
            Times.Once);
    }

    [Test]
    public void whenSetReadyStatus_thenReadyStatusUpdatedAndSaved()
    {
        var user = new UserModel { Id = Id, Name = Name };
        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns(user);

        _userService.SetReadyStatus(Id, true);

        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
        _userRepository.Verify(
            r => r.Save(It.Is<UserModel>(u => u.Id == Id && u.IsReady)),
            Times.Once);
    }

    [Test]
    public void whenRemoveRoomFromAllUsers_thenRoomRemovedAndUsersSaved()
    {
        var users = new List<UserModel>
        {
            new UserModel { Id = Id, Name = Name, RoomId = RoomId },
            new UserModel { Id = Id, Name = Name, RoomId = RoomId }
        };

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(users);

        _userService.RemoveRoomFromAllUsers(RoomId);

        _userRepository.Verify(r => r.FindByRoomId(RoomId), Times.Once);
        _userRepository.Verify(r => r.Save(It.IsAny<UserModel>()), Times.Exactly(users.Count));

        Assert.That(users[0].RoomId, Is.Null);
        Assert.That(users[1].RoomId, Is.Null);
    }

    [Test]
    public void whenUpdateName_thenNameUpdatedAndSaved()
    {
        const string newName = "NEW_NAME";
        var user = new UserModel { Id = Id, Name = Name };

        _userRepository
            .Setup(r => r.FindById(Id))
            .Returns(user);

        _userService.UpdateName(Id, $"  {newName}  ");

        _userRepository.Verify(r => r.FindById(It.Is<long>(l => l == Id)), Times.Once);
        _userRepository.Verify(
            r => r.Save(It.Is<UserModel>(u => u.Id == Id && u.Name == newName)),
            Times.Once);
    }

    [Test]
    public void whenUpdateName_andNameIsEmpty_thenThrowAppException()
    {
        Assert.Throws<AppException>(() => _userService.UpdateName(Id, "   "));

        _userRepository.Verify(r => r.FindById(It.IsAny<long>()), Times.Never);
        _userRepository.Verify(r => r.Save(It.IsAny<UserModel>()), Times.Never);
    }

    private UserModel GetUser(long id, string name, string roomId)
    {
        UserModel user = new UserModel
        {
            Id = id,
            Name = name
        };

        return user;
    }
}
