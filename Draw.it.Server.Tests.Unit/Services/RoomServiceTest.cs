using Draw.it.Server.Enums;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.Room;
using Draw.it.Server.Repositories.User;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.Extensions.Logging;
using Moq;

namespace Draw.it.Server.Tests.Unit.Services;

public class RoomServiceTest
{
    private const string RoomId = "TEST_ROOM_ID";
    private const string OtherRoomId = "OTHER_ROOM_ID";
    private const long UserId = 1;
    private const long OtherUserId = 2;
    private const string UserName = "TEST_USER";
    private const string OtherUserName = "OTHER_USER";
    private const string NewRoomName = "NewName";

    private IRoomService _roomService;
    private Mock<IRoomRepository> _roomRepository = new();
    private Mock<IUserService> _userService = new();
    private Mock<IUserRepository> _userRepository = new();
    private Mock<ILogger<RoomService>> _logger = new();
    
    [SetUp]
    public void Setup()
    {
        _roomRepository = new Mock<IRoomRepository>();
        _userService = new Mock<IUserService>();
        _userRepository = new Mock<IUserRepository>();
        _logger = new Mock<ILogger<RoomService>>();

        _roomService = new RoomService(
            _logger.Object,
            _roomRepository.Object,
            _userService.Object,
            _userRepository.Object
        );
    }

    [Test]
    public void whenCreateRoom_andUserNotInRoom_thenRoomCreatedAndUserAssigned()
    {
        var user = CreateUser(UserId, UserName);

        _roomRepository
            .Setup(r => r.ExistsById(It.IsAny<string>()))
            .Returns(false);

        RoomModel? savedRoom = null;
        _roomRepository
            .Setup(r => r.Save(It.IsAny<RoomModel>()))
            .Callback<RoomModel>(room => savedRoom = room);

        var room = _roomService.CreateRoom(user);

        Assert.That(savedRoom, Is.Not.Null);
        Assert.That(savedRoom.HostId, Is.EqualTo(user.Id));
        Assert.That(room, Is.EqualTo(savedRoom));

        _userService.Verify(s => s.SetRoom(user.Id, savedRoom.Id), Times.Once);
        _userService.Verify(s => s.SetReadyStatus(user.Id, true), Times.Once);
    }

    [Test]
    public void whenCreateRoom_andUserAlreadyInRoom_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, RoomId);

        Assert.Throws<AppException>(() => _roomService.CreateRoom(user));

        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
        _userService.Verify(s => s.SetRoom(It.IsAny<long>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public void whenDeleteRoom_andUserNotInRoom_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, OtherRoomId);

        Assert.Throws<AppException>(() => _roomService.DeleteRoom(RoomId, user));

        _roomRepository.Verify(r => r.FindById(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void whenDeleteRoom_andUserNotHost_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, RoomId);
        var room = CreateRoom(RoomId, OtherUserId);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.DeleteRoom(RoomId, user));

        _userService.Verify(s => s.RemoveRoomFromAllUsers(It.IsAny<string>()), Times.Never);
        _roomRepository.Verify(r => r.DeleteById(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void whenDeleteRoom_andRoomInGame_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, RoomId);
        var room = CreateRoom(RoomId, UserId, RoomStatus.InGame);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.DeleteRoom(RoomId, user));

        _userService.Verify(s => s.RemoveRoomFromAllUsers(It.IsAny<string>()), Times.Never);
        _roomRepository.Verify(r => r.DeleteById(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void whenDeleteRoom_thenRemoveRoomFromUsersAndDelete()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.DeleteById(RoomId))
            .Returns(true);

        _roomService.DeleteRoom(RoomId, user);

        _userService.Verify(s => s.RemoveRoomFromAllUsers(RoomId), Times.Once);
        _roomRepository.Verify(r => r.DeleteById(RoomId), Times.Once);
    }

    [Test]
    public void whenGetRoom_andRoomExists_thenReturnRoom()
    {
        var room = CreateRoom(RoomId, hostId: UserId);
        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.GetRoom(RoomId);

        Assert.That(result, Is.EqualTo(room));
    }

    [Test]
    public void whenGetRoom_andRoomNotFound_thenThrowEntityNotFoundException()
    {
        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns((RoomModel?)null);

        Assert.Throws<EntityNotFoundException>(() => _roomService.GetRoom(RoomId));
    }

    [Test]
    public void whenGetRoomSettings_thenReturnSettings()
    {
        var settings = new RoomSettingsModel();
        var room = CreateRoom(RoomId, UserId, settings: settings);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.GetRoomSettings(RoomId);

        Assert.That(result, Is.EqualTo(settings));
    }

    [Test]
    public void whenGetUsersInRoom_andRoomNotFound_thenThrowEntityNotFoundException()
    {
        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(false);

        Assert.Throws<EntityNotFoundException>(() => _roomService.GetUsersInRoom(RoomId));

        _userRepository.Verify(r => r.FindByRoomId(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void whenGetUsersInRoom_thenReturnUsersFromRepository()
    {
        var users = new List<UserModel>
        {
            CreateUser(UserId, UserName, roomId: RoomId),
            CreateUser(OtherUserId, OtherUserName, roomId: RoomId)
        };

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(users);

        var result = _roomService.GetUsersInRoom(RoomId).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Contain(users[0]));
        Assert.That(result, Does.Contain(users[1]));
    }

    [Test]
    public void whenJoinRoom_andUserAlreadyInRoom_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: OtherRoomId);

        Assert.Throws<AppException>(() => _roomService.JoinRoom(RoomId, user));

        _roomRepository.Verify(r => r.FindById(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void whenJoinRoom_andRoomNotInLobby_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: null);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InGame);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.JoinRoom(RoomId, user));
    }

    [Test]
    public void whenJoinRoom_andUsernameAlreadyExists_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: null);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        var existingPlayer = CreateUser(OtherUserId, UserName, roomId: RoomId);
        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(new List<UserModel> { existingPlayer });

        Assert.Throws<AppException>(() => _roomService.JoinRoom(RoomId, user));

        _userService.Verify(s => s.SetRoom(It.IsAny<long>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public void whenJoinRoom_thenUserAssignedToRoomAndSetNotReady()
    {
        var user = CreateUser(UserId, UserName, roomId: null);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(new List<UserModel>());

        _roomService.JoinRoom(RoomId, user);

        _userService.Verify(s => s.SetRoom(UserId, RoomId), Times.Once);
        _userService.Verify(s => s.SetReadyStatus(UserId, false), Times.Once);
    }
    
    [Test]
    public void whenLeaveRoom_andUserNotInRoom_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: OtherRoomId);

        Assert.Throws<AppException>(() => _roomService.LeaveRoom(RoomId, user));
    }

    [Test]
    public void whenLeaveRoom_andUserIsHost_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.LeaveRoom(RoomId, user));

        _userService.Verify(s => s.SetRoom(It.IsAny<long>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public void whenLeaveRoom_andRoomInGame_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InGame);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.LeaveRoom(RoomId, user));

        _userService.Verify(s => s.SetRoom(It.IsAny<long>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public void whenLeaveRoom_thenUserRemovedFromRoom()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomService.LeaveRoom(RoomId, user);

        _userService.Verify(s => s.SetRoom(UserId, null), Times.Once);
    }
    

    [Test]
    public void whenIsHost_andUserNotInRoom_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: OtherRoomId);

        Assert.Throws<AppException>(() => _roomService.IsHost(RoomId, user));
    }

    [Test]
    public void whenIsHost_andUserIsHost_thenReturnTrue()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.IsHost(RoomId, user);

        Assert.That(result, Is.True);
    }

    [Test]
    public void whenIsHost_andUserIsNotHost_thenReturnFalse()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.IsHost(RoomId, user);

        Assert.That(result, Is.False);
    }

    [Test]
    public void whenStartGame_andUserNotHost_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.StartGame(RoomId, user));
    }

    [Test]
    public void whenStartGame_andRoomNotInLobby_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InGame);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.StartGame(RoomId, user));
    }

    [Test]
    public void whenStartGame_andLessThanTwoPlayers_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        var players = new List<UserModel>
        {
            CreateUser(UserId, UserName, roomId: RoomId, isReady: true)
        };

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(players);

        Assert.Throws<AppException>(() => _roomService.StartGame(RoomId, user));

        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
    }

    [Test]
    public void whenStartGame_andSomePlayersNotReady_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        var players = new List<UserModel>
        {
            CreateUser(UserId, UserName, roomId: RoomId, isReady: true),
            CreateUser(OtherUserId, OtherUserName, roomId: RoomId, isReady: false)
        };

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(players);

        Assert.Throws<AppException>(() => _roomService.StartGame(RoomId, user));

        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
    }

    [Test]
    public void whenStartGame_thenStatusSetToInGameAndRoomSaved()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        var players = new List<UserModel>
        {
            CreateUser(UserId, UserName, roomId: RoomId, isReady: true),
            CreateUser(OtherUserId, OtherUserName, roomId: RoomId, isReady: true)
        };

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(players);

        _roomService.StartGame(RoomId, user);

        Assert.That(room.Status, Is.EqualTo(RoomStatus.InGame));
        _roomRepository.Verify(r => r.Save(room), Times.Once);
    }

    [Test]
    public void whenStartGame_andAiPlayer_thenStatusSetToInGameAndRoomSavedAndAiPlayerCreated()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby);
        room.Settings.HasAiPlayer = true;

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        _roomRepository
            .Setup(r => r.ExistsById(RoomId))
            .Returns(true);

        var players = new List<UserModel>
        {
            CreateUser(UserId, UserName, roomId: RoomId, isReady: true),
            CreateUser(OtherUserId, OtherUserName, roomId: RoomId, isReady: true)
        };

        _userRepository
            .Setup(r => r.FindByRoomId(RoomId))
            .Returns(players);

        _roomService.StartGame(RoomId, user);

        Assert.That(room.Status, Is.EqualTo(RoomStatus.InGame));
        _roomRepository.Verify(r => r.Save(room), Times.Once);
        _userService.Verify(r => r.CreateAiUser(room.Id), Times.Once);
    }
    
    [Test]
    public void whenUpdateSettings_andSettingsSame_thenReturnFalseAndDoNotSave()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var existingSettings = new RoomSettingsModel();
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby, settings: existingSettings);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.UpdateSettings(RoomId, user, existingSettings);

        Assert.That(result, Is.False);
        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
    }

    [Test]
    public void whenUpdateSettings_andUserNotHost_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var existingSettings = new RoomSettingsModel();
        var newSettings = new RoomSettingsModel { RoomName = NewRoomName };
        var room = CreateRoom(RoomId, hostId: OtherUserId, status: RoomStatus.InLobby, settings: existingSettings);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.UpdateSettings(RoomId, user, newSettings));

        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
    }

    [Test]
    public void whenUpdateSettings_andRoomNotInLobby_thenThrowAppException()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var existingSettings = new RoomSettingsModel();
        var newSettings = new RoomSettingsModel { RoomName = NewRoomName };
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InGame, settings: existingSettings);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        Assert.Throws<AppException>(() => _roomService.UpdateSettings(RoomId, user, newSettings));

        _roomRepository.Verify(r => r.Save(It.IsAny<RoomModel>()), Times.Never);
    }

    [Test]
    public void whenUpdateSettings_thenSettingsUpdatedAndSavedAndReturnTrue()
    {
        var user = CreateUser(UserId, UserName, roomId: RoomId);
        var existingSettings = new RoomSettingsModel();
        var newSettings = new RoomSettingsModel { RoomName = NewRoomName };
        var room = CreateRoom(RoomId, hostId: UserId, status: RoomStatus.InLobby, settings: existingSettings);

        _roomRepository
            .Setup(r => r.FindById(RoomId))
            .Returns(room);

        var result = _roomService.UpdateSettings(RoomId, user, newSettings);

        Assert.That(result, Is.True);
        Assert.That(room.Settings, Is.EqualTo(newSettings));
        _roomRepository.Verify(r => r.Save(room), Times.Once);
    }

    private static UserModel CreateUser(long id, string name, string? roomId = null, bool isReady = false)
    {
        return new UserModel
        {
            Id = id,
            Name = name,
            RoomId = roomId,
            IsConnected = false,
            IsReady = isReady
        };
    }

    private static RoomModel CreateRoom(
        string roomId,
        long hostId,
        RoomStatus status = RoomStatus.InLobby,
        RoomSettingsModel? settings = null)
    {
        return new RoomModel
        {
            Id = roomId,
            HostId = hostId,
            Status = status,
            Settings = settings ?? new RoomSettingsModel()
        };
    }
}
