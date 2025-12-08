using System.Reflection;
using System.Security.Claims;
using Draw.it.Server.Hubs;
using Draw.it.Server.Hubs.DTO;
using Draw.it.Server.Models.Game;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Draw.it.Server.Tests.Unit.Hubs;

public class GameplayHubTest
{
    private const long UserId = 1;
    private const string RoomId = "ABC123";

    private Mock<ILogger<GameplayHub>> _logger;
    private Mock<IUserService> _userService;
    private Mock<IGameService> _gameService;
    private Mock<IRoomService> _roomService;
    private Mock<HubCallerContext> _context;
    private Mock<IHubCallerClients> _clients;
    private Mock<ISingleClientProxy> _callerClient;
    private Mock<IClientProxy> _groupClient;
    private Mock<IClientProxy> _userClient;
    private Mock<IClientProxy> _groupExceptClient;
    private Mock<IGroupManager> _groups;

    private UserModel _user;
    private TestableGameplayHub _hub;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GameplayHub>>();
        _userService = new Mock<IUserService>();
        _gameService = new Mock<IGameService>();
        _roomService = new Mock<IRoomService>();
        _context = new Mock<HubCallerContext>();
        _clients = new Mock<IHubCallerClients>();
        _callerClient = new Mock<ISingleClientProxy>();
        _groupClient = new Mock<IClientProxy>();
        _userClient = new Mock<IClientProxy>();
        _groupExceptClient = new Mock<IClientProxy>();
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
        _clients.Setup(c => c.User(It.IsAny<string>())).Returns(_userClient.Object);
        _clients.Setup(c => c.GroupExcept(
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>()))
            .Returns(_groupExceptClient.Object);

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

        _userClient
            .Setup<Task>(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _groupExceptClient
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

        _hub = new TestableGameplayHub(
            _logger.Object,
            _userService.Object,
            _gameService.Object,
            _roomService.Object);

        _hub.SetContext(_context.Object);
        _hub.SetClients(_clients.Object);
        _hub.SetGroups(_groups.Object);
    }

    private class TestableGameplayHub : GameplayHub
    {
        public TestableGameplayHub(
            ILogger<GameplayHub> logger,
            IUserService userService,
            IGameService gameService,
            IRoomService roomService)
            : base(logger, userService, gameService, roomService)
        {
        }

        public void SetContext(HubCallerContext context) => Context = context;
        public void SetClients(IHubCallerClients clients) => Clients = clients;
        public void SetGroups(IGroupManager groups) => Groups = groups;
    }

    [TearDown]
    public void TearDown()
    {
        _hub.Dispose();
    }

    [Test]
    public async Task whenOnConnected_andWaitingForPlayers_andNewPlayer_thenWaitingMessageSentToCaller()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2 }, 2, "APPLE");

        _gameService
            .Setup(s => s.AddConnectedPlayer(RoomId, UserId))
            .Returns(true);

        await _hub.OnConnectedAsync();

        VerifyAddedToGroupOnce();

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == "System" &&
                    ((string)args[1]!).Contains($"{_user.Name} joined the game")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == "System" &&
                    ((string)args[1]!).Contains($"Waiting for other players to connect... ({game.ConnectedPlayersIds.Count}/{game.PlayerCount})")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenOnConnected_andWaitingForPlayers_andReconnected_thenWaitingMessageSentToCaller()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2 }, 2, "APPLE");

        _gameService
            .Setup(s => s.AddConnectedPlayer(RoomId, UserId))
            .Returns(false);

        await _hub.OnConnectedAsync();

        VerifyAddedToGroupOnce();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == "System" &&
                    ((string)args[1]!).Contains($"Waiting for other players to connect... ({game.ConnectedPlayersIds.Count}/{game.PlayerCount})")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenOnConnected_andGameStarted_andNewPlayer_thenStartTurn()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2, 3 }, 2, "APPLE");

        _gameService
            .Setup(s => s.GetMaskedWord("APPLE"))
            .Returns("_____");

        _gameService
            .Setup(s => s.AddConnectedPlayer(RoomId, UserId))
            .Returns(true);

        _userService
            .Setup(s => s.GetUser(game.CurrentDrawerId))
            .Returns(new UserModel
            {
                Id = 2,
                Name = "DRAWER_USER",
                RoomId = RoomId
            });

        var room = CreateRoom(2, 3);

        await _hub.OnConnectedAsync();

        VerifyAddedToGroupOnce();

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == "System" &&
                    ((string)args[1]!).Contains($"ROUND {game.CurrentRound}/{room.Settings.NumberOfRounds} STARTED!")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _groupExceptClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.Is<object?[]>(args =>
                    args.Length == 1 &&
                    (string)args[0]! == "_____"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.Is<object?[]>(args =>
                    args.Length == 1 &&
                    (string)args[0]! == "APPLE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenOnConnected_andGameStarted_andReconnected_andUserIsDrawer_thenSendWordToCaller()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2, 3 }, UserId, "APPLE");

        _gameService
            .Setup(s => s.AddConnectedPlayer(RoomId, UserId))
            .Returns(false);

        await _hub.OnConnectedAsync();

        VerifyAddedToGroupOnce();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.Is<object?[]>(args =>
                    args.Length == 1 &&
                    (string)args[0]! == "APPLE"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenOnConnected_andGameStarted_andReconnected_andUserIsNotDrawer_thenSendMaskedWordToCaller()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2, 3 }, 2, "APPLE");

        _gameService
            .Setup(s => s.GetMaskedWord("APPLE"))
            .Returns("_____");

        _gameService
            .Setup(s => s.AddConnectedPlayer(RoomId, UserId))
            .Returns(false);

        await _hub.OnConnectedAsync();

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.Is<object?[]>(args =>
                    args.Length == 1 &&
                    (string)args[0]! == "_____"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task whenSendMessage_andSenderIsDrawer_thenNormalMessageBroadcast()
    {
        var game = CreateGame(2, new HashSet<long>(), UserId, "APPLE");

        const string message = "hello everyone";

        await _hub.SendMessage(message);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == _user.Name &&
                    (string)args[1]! == message),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _gameService.Verify(
            s => s.AddGuessedPlayer(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny),
            Times.Never);
    }

    [Test]
    public async Task whenSendMessage_andSenderIsNotDrawer_andWrongGuess_thenNormalMessageBroadcast()
    {
        var game = CreateGame(2, new HashSet<long>(), 2, "APPLE");

        const string message = "banana";

        await _hub.SendMessage(message);

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 2 &&
                    (string)args[0]! == _user.Name &&
                    (string)args[1]! == message),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _gameService.Verify(
            s => s.AddGuessedPlayer(It.IsAny<string>(), It.IsAny<long>(), out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny),
            Times.Never);
    }

    [Test]
    public async Task whenSendMessage_andSenderIsNotDrawer_andCorrectGuess_thenMessageBroadcastCorrect()
    {
        var game = CreateGame(3, new HashSet<long> { UserId, 2, 3 }, 2, "APPLE");

        SetupAddGuessedPlayerCallback(false, false, false);

        var room = CreateRoom(2, 3);

        await _hub.SendMessage("APPLE");

        _groupClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveMessage",
                It.Is<object?[]>(args =>
                    args.Length == 3 &&
                    (string)args[0]! == _user.Name &&
                    (string)args[1]! == "Guessed The Word!" &&
                    (bool)args[2]! == true),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _callerClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveWordToDraw",
                It.Is<object?[]>(args =>
                    args.Length == 1 &&
                    (string)args[0]! == "APPLE"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenSendDraw_thenBroadcastToGroupExceptCaller()
    {
        await _hub.SendDraw(null!);

        _groupExceptClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveDraw",
                It.Is<object?[]>(args => args.Length == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task whenSendClear_thenBroadcastClearToGroupExceptCaller()
    {
        await _hub.SendClear();

        _groupExceptClient.Verify(
            c => c.SendCoreAsync(
                "ReceiveClear",
                It.Is<object?[]>(args => args.Length == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void GetPlayerStatuses_ReturnsCorrectStatusesOrderedByScore()
    {
        var drawerId = 2;

        var game = CreateGame(
            playerCount: 3,
            connectedPlayersIds: new HashSet<long> { 1, 2, 3 },
            currentDrawerId: drawerId,
            wordToDraw: "APPLE"
        );

        game.TotalScores = new Dictionary<long, int> { [1] = 5, [2] = 10, [3] = 7 };
        game.RoundScores = new Dictionary<long, int> { [1] = 2, [2] = 1, [3] = 3 };
        game.GuessedPlayersIds = new List<long> { 1, 3 };

        var users = new List<UserModel>
        {
            new UserModel { Id = 1, Name = "Alice" },
            new UserModel { Id = 2, Name = "Bob" },
            new UserModel { Id = 3, Name = "Charlie" }
        };
        _roomService.Setup(s => s.GetUsersInRoom(RoomId)).Returns(users);

        var method = typeof(GameplayHub)
            .GetMethod("GetPlayerStatuses", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (List<PlayerStatusDto>)method.Invoke(_hub, new object[] { RoomId })!;

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Name, Is.EqualTo("Bob"));
        Assert.That(result[0].Score, Is.EqualTo(11));
        Assert.That(result[0].IsDrawer, Is.True);
        Assert.That(result[0].HasGuessed, Is.False);

        Assert.That(result[1].Name, Is.EqualTo("Charlie"));
        Assert.That(result[1].Score, Is.EqualTo(10));
        Assert.That(result[1].IsDrawer, Is.False);
        Assert.That(result[1].HasGuessed, Is.True);

        Assert.That(result[2].Name, Is.EqualTo("Alice"));
        Assert.That(result[2].Score, Is.EqualTo(7));
        Assert.That(result[2].IsDrawer, Is.False);
        Assert.That(result[2].HasGuessed, Is.True);
    }

    // Helper builders and setup methods to reduce duplication across tests
    private GameModel CreateGame(
        int playerCount,
        HashSet<long> connectedPlayersIds,
        long currentDrawerId,
        string wordToDraw,
        int currentRound = 1)
    {
        var game = new GameModel
        {
            RoomId = RoomId,
            PlayerCount = playerCount,
            ConnectedPlayersIds = connectedPlayersIds,
            CurrentDrawerId = currentDrawerId,
            WordToDraw = wordToDraw,
            CurrentRound = currentRound
        };

        _gameService
            .Setup(s => s.GetGame(RoomId))
            .Returns(game);

        return game;
    }

    private RoomModel CreateRoom(int hostId, int numberOfRounds)
    {
        var room = new RoomModel
        {
            Id = RoomId,
            HostId = hostId,
            Settings = new RoomSettingsModel
            {
                NumberOfRounds = numberOfRounds
            }
        };

        _roomService
            .Setup(s => s.GetRoom(RoomId))
            .Returns(room);

        return room;
    }

    private void SetupAddGuessedPlayerCallback(bool turnEnded, bool roundEnded, bool gameEnded)
    {
        _gameService
            .Setup(s => s.AddGuessedPlayer(RoomId, UserId, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny))
            .Callback((string roomId, long userId, out bool turn, out bool round, out bool game) =>
            {
                turn = turnEnded;
                round = roundEnded;
                game = gameEnded;
            });
    }

    private void VerifyAddedToGroupOnce()
    {
        _groups.Verify(
            g => g.AddToGroupAsync(
                "connection-1",
                RoomId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
