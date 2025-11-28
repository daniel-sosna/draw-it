using System.Reflection;
using System.Security.Claims;
using Draw.it.Server.Controllers.Room;
using Draw.it.Server.Controllers.Room.DTO;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace draw_it.Tests.Controllers;

public class RoomControllerTest
{
    private const long UserId = 1;
    private const string UserName = "TEST_USER";
    private const string RoomId = "ROOM_1";

    private RoomController _roomController;
    private DefaultHttpContext _httpContext;
    private Mock<IRoomService> _roomService = new();
    private Mock<IUserService> _userService = new();

    [SetUp]
    public void Setup()
    {
        _roomService = new Mock<IRoomService>();
        _userService = new Mock<IUserService>();
        _httpContext = new DefaultHttpContext();

        _roomController = new RoomController(_roomService.Object, _userService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Test]
    public void whenCreateRoom_thenRoomCreatedAndCreatedResultReturned()
    {
        var user = SetupAuthenticatedUser();
        var room = new RoomModel
        {
            Id = RoomId,
            HostId = user.Id
        };

        _roomService
            .Setup(s => s.CreateRoom(user))
            .Returns(room);

        var result = _roomController.CreateRoom();

        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var created = (CreatedResult)result;

        Assert.That(created.Location, Is.EqualTo($"api/v1/host/{RoomId}"));
        Assert.That(created.Value, Is.InstanceOf<RoomCreateResponseDto>());

        var dto = (RoomCreateResponseDto)created.Value!;
        Assert.That(dto.roomId, Is.EqualTo(RoomId));

        _roomService.Verify(s => s.CreateRoom(user), Times.Once);
    }

    [Test]
    public void whenJoinRoom_thenServiceCalledAndNoContentReturned()
    {
        var user = SetupAuthenticatedUser();

        var result = _roomController.JoinRoom(RoomId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _roomService.Verify(s => s.JoinRoom(RoomId, user), Times.Once);
    }

    [Test]
    public void whenLeaveRoom_thenServiceCalledAndNoContentReturned()
    {
        var user = SetupAuthenticatedUser();

        var result = _roomController.LeaveRoom(RoomId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _roomService.Verify(s => s.LeaveRoom(RoomId, user), Times.Once);
    }

    [Test]
    public void whenGetRoom_thenReturnRoom()
    {
        var room = new RoomModel
        {
            Id = RoomId,
            HostId = UserId
        };

        _roomService
            .Setup(s => s.GetRoom(RoomId))
            .Returns(room);

        var result = _roomController.GetRoom(RoomId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;

        Assert.That(ok.Value, Is.EqualTo(room));
        _roomService.Verify(s => s.GetRoom(RoomId), Times.Once);
    }

    [Test]
    public void whenDeleteRoom_thenServiceCalledAndNoContentReturned()
    {
        var user = SetupAuthenticatedUser();

        var result = _roomController.DeleteRoom(RoomId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _roomService.Verify(s => s.DeleteRoom(RoomId, user), Times.Once);
    }

    [Test]
    public void whenGetRoomUsers_thenReturnUsers()
    {
        var users = new List<UserModel>
        {
            new() { Id = 1, Name = "User1" },
            new() { Id = 2, Name = "User2" }
        };

        _roomService
            .Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(users);

        var result = _roomController.GetRoomUsers(RoomId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var ok = (OkObjectResult)result;

        Assert.That(ok.Value, Is.EqualTo(users));
        _roomService.Verify(s => s.GetUsersInRoom(RoomId), Times.Once);
    }

    [Test]
    public void whenStartGame_thenServiceCalledAndNoContentReturned()
    {
        var user = SetupAuthenticatedUser();

        var result = _roomController.StartGame(RoomId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _roomService.Verify(s => s.StartGame(RoomId, user), Times.Once);
    }

    [Test]
    public void whenUpdateSettings_thenServiceCalledAndNoContentReturned()
    {
        var user = SetupAuthenticatedUser();
        var settings = new RoomSettingsModel();

        var result = _roomController.UpdateSettings(RoomId, settings);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _roomService.Verify(s => s.UpdateSettings(RoomId, user, settings), Times.Once);
    }

    [Test]
    public void whenRoomControllerClass_thenHasAuthorizeAttribute()
    {
        var attr = typeof(RoomController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.That(attr, Is.Not.Null);
    }

    [Test]
    public void whenGetRoom_thenHasAllowAnonymousAttribute()
    {
        var method = typeof(RoomController).GetMethod(nameof(RoomController.GetRoom));
        Assert.That(method, Is.Not.Null);

        var attr = method!.GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.That(attr, Is.Not.Null);
    }

    private UserModel SetupAuthenticatedUser()
    {
        var user = new UserModel
        {
            Id = UserId,
            Name = UserName
        };

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString())
        });
        _httpContext.User = new ClaimsPrincipal(identity);

        _userService
            .Setup(s => s.GetUser(UserId))
            .Returns(user);

        return user;
    }
}
