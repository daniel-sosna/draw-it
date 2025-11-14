using System.Net;
using System.Security.Claims;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Hubs;
using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace draw_it.Tests.Hubs;

public class LobbyHubTest
{
    private const long UserId = 1;
    private const string RoomId = "ROOM_1";

    private Mock<ILogger<LobbyHub>> _logger;
    private Mock<IRoomService> _roomService;
    private Mock<IUserService> _userService;
    private Mock<IGameService> _gameService;
    private Mock<HubCallerContext> _context;
    private Mock<IHubCallerClients> _clients;
    private Mock<ISingleClientProxy> _callerClient;
    private Mock<IClientProxy> _groupClient;
    private Mock<IGroupManager> _groups;

    private UserModel _user;
    private TestableLobbyHub _hub;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<LobbyHub>>();
        _roomService = new Mock<IRoomService>();
        _userService = new Mock<IUserService>();
        _gameService = new Mock<IGameService>();
        _context = new Mock<HubCallerContext>();
        _clients = new Mock<IHubCallerClients>();
        _callerClient = new Mock<ISingleClientProxy>();
        _groupClient = new Mock<IClientProxy>();
        _groups = new Mock<IGroupManager>();

        _user = new UserModel
        {
            Id = UserId,
            Name = "TEST_USER",
            RoomId = RoomId
        };

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, UserId.ToString()) },
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _context.SetupGet(c => c.User).Returns(principal);
        _context.SetupGet(c => c.ConnectionId).Returns("connection-1");
        _context.SetupGet(c => c.UserIdentifier).Returns(UserId.ToString());
        
        _userService
            .Setup(s => s.GetUser(UserId))
            .Returns(_user);

        _clients.Setup(c => c.Caller).Returns(_callerClient.Object);
        _clients.Setup(c => c.Group(It.IsAny<string>())).Returns(_groupClient.Object);

        _callerClient
            .Setup<Task>(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        _groupClient
            .Setup<Task>(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _groups
            .Setup(g => g.AddToGroupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _hub = new TestableLobbyHub(
            _logger.Object,
            _roomService.Object,
            _userService.Object,
            _gameService.Object);

        _hub.SetContext(_context.Object);
        _hub.SetClients(_clients.Object);
        _hub.SetGroups(_groups.Object);
    }

    // Helper subclass to allow setting Context/Clients from tests.
    private class TestableLobbyHub : LobbyHub
    {
        public TestableLobbyHub(
            ILogger<LobbyHub> logger,
            IRoomService roomService,
            IUserService userService,
            IGameService gameService)
            : base(logger, roomService, userService, gameService)
        {
        }

        public void SetContext(HubCallerContext context) => Context = context;
        public void SetClients(IHubCallerClients clients) => Clients = clients;
        public void SetGroups(IGroupManager groups) => Groups = groups;
    }
    
    [Test]
    public async Task whenOnConnected_andUserIsNotHost_thenSettingsAndPlayerListSent()
    {
        var settings = new RoomSettingsModel();

        _roomService
            .Setup(s => s.IsHost(RoomId, It.IsAny<UserModel>()))
            .Returns(false);

        _roomService
            .Setup(s => s.GetRoomSettings(RoomId))
            .Returns(settings);

        _roomService
            .Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(new List<UserModel> { _user });

        await _hub.OnConnectedAsync();

        _userService.Verify(s => s.SetConnectedStatus(UserId, true), Times.Once);
        _roomService.Verify(s => s.GetRoomSettings(RoomId), Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveUpdateSettings",
                It.Is<object?[]>(args => args.Length == 1 && args[0] is SettingsDto),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceivePlayerList",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenOnConnected_andUserIsHost_thenNoSettingsSent()
    {
        _roomService
            .Setup(s => s.IsHost(RoomId, It.IsAny<UserModel>()))
            .Returns(true);

        await _hub.OnConnectedAsync();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveUpdateSettings",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceivePlayerList",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenLeaveRoom_andUserHasNoRoom_thenThrowHubException()
    {
        _user.RoomId = null;

        Assert.ThrowsAsync<HubException>(async () => await _hub.LeaveRoom());

        _roomService.Verify(
            s => s.IsHost(It.IsAny<string>(), It.IsAny<UserModel>()),
            Times.Never);
        _roomService.Verify(
            s => s.LeaveRoom(It.IsAny<string>(), It.IsAny<UserModel>()),
            Times.Never);
        _roomService.Verify(
            s => s.DeleteRoom(It.IsAny<string>(), It.IsAny<UserModel>()),
            Times.Never);
    }

    [Test]
    public async Task whenLeaveRoom_andUserIsHost_thenRoomDeletedAndRoomDeletedEventSent()
    {
        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(true);

        await _hub.LeaveRoom();

        _roomService.Verify(s => s.DeleteRoom(RoomId, _user), Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveRoomDeleted",
                It.Is<object?[]>(args => args.Length == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenLeaveRoom_andUserIsNotHost_thenLeaveRoomAndPlayerListSent()
    {
        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(false);

        _roomService
            .Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(new List<UserModel> { _user });

        await _hub.LeaveRoom();

        _roomService.Verify(s => s.LeaveRoom(RoomId, _user), Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceivePlayerList",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void whenLeaveRoom_andAppExceptionThrown_thenHubException()
    {
        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(false);

        _roomService
            .Setup(s => s.LeaveRoom(RoomId, _user))
            .Throws(new AppException("boom", HttpStatusCode.Conflict));

        Assert.ThrowsAsync<HubException>(async () => await _hub.LeaveRoom());
    }

    [Test]
    public void whenLeaveRoom_andUnexpectedExceptionThrown_thenHubException()
    {
        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(false);

        _roomService
            .Setup(s => s.LeaveRoom(RoomId, _user))
            .Throws(new Exception("oops"));

        Assert.ThrowsAsync<HubException>(async () => await _hub.LeaveRoom());
    }

    [Test]
    public async Task whenUpdateRoomSettings_andUpdated_thenBroadcastSettings()
    {
        var settings = new RoomSettingsModel();

        _roomService
            .Setup(s => s.UpdateSettings(RoomId, _user, settings))
            .Returns(true);

        await _hub.UpdateRoomSettings(settings);

        _roomService.Verify(s => s.UpdateSettings(RoomId, _user, settings), Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveUpdateSettings",
                It.Is<object?[]>(args => args.Length == 1 && args[0] is SettingsDto),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenUpdateRoomSettings_andNotUpdated_thenNoBroadcast()
    {
        var settings = new RoomSettingsModel();

        _roomService
            .Setup(s => s.UpdateSettings(RoomId, _user, settings))
            .Returns(false);

        await _hub.UpdateRoomSettings(settings);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveUpdateSettings",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenSendPlayerListUpdate_thenPlayerListSentToGroup()
    {
        var otherUser = new UserModel
        {
            Id = 2,
            Name = "OTHER",
            RoomId = RoomId
        };

        _roomService
            .Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(new List<UserModel> { _user, otherUser });

        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(true);
        _roomService
            .Setup(s => s.IsHost(RoomId, otherUser))
            .Returns(false);

        await _hub.SendPlayerListUpdate(RoomId);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceivePlayerList",
                It.Is<object?[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenSetPlayerReady_thenReadyStatusUpdatedAndPlayerListSent()
    {
        _roomService
            .Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(new List<UserModel> { _user });
        _roomService
            .Setup(s => s.IsHost(RoomId, _user))
            .Returns(true);

        await _hub.SetPlayerReady(true);

        _userService.Verify(s => s.SetReadyStatus(UserId, true), Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceivePlayerList",
                It.Is<object?[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenStartGame_andSuccess_thenServicesCalledAndGameStartBroadcast()
    {
        _roomService
            .Setup(s => s.StartGame(RoomId, _user));
        _gameService
            .Setup(g => g.CreateGame(RoomId));

        await _hub.StartGame();

        _roomService.Verify(s => s.StartGame(RoomId, _user), Times.Once);
        _gameService.Verify(g => g.CreateGame(RoomId), Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveGameStart",
                It.Is<object?[]>(args => args.Length == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveErrorOnGameStart",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenStartGame_andAppException_thenErrorSentToCallerNoGameStart()
    {
        var appEx = new AppException("Game cannot start", HttpStatusCode.Conflict);

        _roomService
            .Setup(s => s.StartGame(RoomId, _user))
            .Throws(appEx);

        await _hub.StartGame();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveErrorOnGameStart",
                It.Is<object?[]>(args =>
                    args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _gameService.Verify(g => g.CreateGame(It.IsAny<string>()), Times.Never);
        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveGameStart",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenStartGame_andUnexpectedException_thenGenericErrorSent()
    {
        _roomService
            .Setup(s => s.StartGame(RoomId, _user))
            .Throws(new Exception("oops"));

        await _hub.StartGame();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveErrorOnGameStart",
                It.Is<object?[]>(args =>
                    args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveGameStart",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        _hub.Dispose();
    }
}
