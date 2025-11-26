using Draw.it.Server.Enums;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Models.Game;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Models.WordPool;
using Draw.it.Server.Repositories.Game;
using Draw.it.Server.Services.Game;
using Draw.it.Server.Services.Room;
using Draw.it.Server.Services.WordPool;
using Microsoft.Extensions.Logging;
using Moq;

namespace draw_it.Tests.Services;

public class GameServiceTest
{
    private const string RoomId = "ROOM_1";
    private const long DrawerId = 1;
    private const long Player2Id = 2;
    private const long CategoryId = 10;
    private const string Name = "NAME";

    private Mock<ILogger<GameService>> _logger = null!;
    private Mock<IGameRepository> _repo = null!;
    private Mock<IRoomService> _roomService = null!;
    private Mock<IWordPoolService> _wordPool = null!;

    private GameService _service = null!;

    private GameModel _game = null!;
    private RoomModel _room = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GameService>>();
        _repo = new Mock<IGameRepository>();
        _roomService = new Mock<IRoomService>();
        _wordPool = new Mock<IWordPoolService>();

        _service = new GameService(_logger.Object, _repo.Object, _roomService.Object, _wordPool.Object);

        _game = new GameModel
        {
            RoomId = RoomId,
            PlayerCount = 2,
            CurrentDrawerId = DrawerId,
            CurrentRound = 1,
            CurrentTurnIndex = 0,
            WordToDraw = "APPLE",
            GuessedPlayersIds = new List<long>()
        };

        _room = new RoomModel
        {
            Id = RoomId,
            HostId = DrawerId,
            Status = RoomStatus.InGame,
            Settings = new RoomSettingsModel
            {
                NumberOfRounds = 3,
                CategoryId = CategoryId
            }
        };

        _repo.Setup(r => r.FindById(RoomId)).Returns(_game);
        _roomService.Setup(r => r.GetRoom(RoomId)).Returns(_room);
        _wordPool.Setup(s => s.GetRandomWordByCategoryId(It.IsAny<long>()))
             .Returns((long categoryId) => new WordModel { CategoryId = categoryId, Value = "BANANA" });
    }

    [Test]
    public void whenGetGame_andExists_thenReturnGame()
    {
        var result = _service.GetGame(RoomId);
        Assert.That(result, Is.EqualTo(_game));
    }

    [Test]
    public void whenGetGame_andNotExists_thenThrow()
    {
        _repo.Setup(r => r.FindById(RoomId)).Returns((GameModel?)null);
        Assert.Throws<EntityNotFoundException>(() => _service.GetGame(RoomId));
    }

    [Test]
    public void whenDeleteGame_andFails_thenThrow()
    {
        _repo.Setup(r => r.DeleteById(RoomId)).Returns(false);
        Assert.Throws<EntityNotFoundException>(() => _service.DeleteGame(RoomId));
    }

    [Test]
    public void whenCreateGame_withInvalidRoomStatus_thenThrow()
    {
        _room.Status = RoomStatus.InLobby;
        Assert.Throws<AppException>(() => _service.CreateGame(RoomId));
    }

    [Test]
    public void whenCreateGame_thenGameIsCreatedSavedWithCorrectInitialState()
    {
        var players = new List<UserModel>
        {
            new UserModel { Id = DrawerId, Name = "A" },
            new UserModel { Id = Player2Id, Name = "B" }
        };

        _roomService.Setup(s => s.GetUsersInRoom(RoomId)).Returns(players);

        _wordPool.Setup(s => s.GetRandomWordByCategoryId(CategoryId))
                 .Returns(new WordModel { CategoryId = CategoryId, Value = "BANANA" });

        _service.CreateGame(RoomId);

        _repo.Verify(r => r.Save(It.Is<GameModel>(g =>
            g.RoomId == RoomId &&
            g.CurrentRound == 1 &&
            g.CurrentTurnIndex == 0 &&
            g.CurrentDrawerId == DrawerId &&
            g.WordToDraw == "BANANA"
        )), Times.Once);
    }

    [Test]
    public void whenGetRandomWord_thenReturnsValue()
    {
        _wordPool.Setup(s => s.GetRandomWordByCategoryId(CategoryId))
                 .Returns(new WordModel { CategoryId = CategoryId, Value = "CAT" });

        var w = _service.GetRandomWord(CategoryId);

        Assert.That(w, Is.EqualTo("CAT"));
    }
    
    [Test]
    public void whenGetDrawerId_thenReturnCurrentDrawer()
    {
        var result = _service.GetDrawerId(RoomId);
        Assert.That(result, Is.EqualTo(DrawerId));
    }

    [Test]
    public void whenAddGuessedPlayer_firstTime_thenSavedAndTurnAdvanced()
    {
        _roomService.Setup(s => s.GetUsersInRoom(RoomId))
            .Returns(new List<UserModel>
            {
                new UserModel {Id = DrawerId, Name = Name},
                new UserModel {Id = Player2Id, Name = Name}
            });

        _service.AddGuessedPlayer(RoomId, Player2Id, out bool turnEnded, out bool roundEnded, out bool gameEnded);

        Assert.That(turnEnded, Is.True); // Only 1 guesser needed (2 players)
        Assert.That(roundEnded, Is.False); // 2 turns per round (2 players)
        Assert.That(gameEnded, Is.False); // Not the end of the round
    }

    [Test]
    public void whenAddGuessedPlayer_duplicate_thenNotSaved()
    {
        _game.GuessedPlayersIds.Add(Player2Id);

        _service.AddGuessedPlayer(RoomId, Player2Id, out bool turnEnded, out bool roundEnded, out bool gameEnded);

        Assert.That(turnEnded, Is.False);
        Assert.That(roundEnded, Is.False);
        Assert.That(gameEnded, Is.False);
        _repo.Verify(r => r.Save(It.IsAny<GameModel>()), Times.Never);
    }

    [Test]
    public void whenGetMaskedWord_thenMaskNonSpaces()
    {
        var masked = _service.GetMaskedWord("DOG CAT");
        Assert.That(masked, Is.EqualTo("*** ***"));
    }

    [Test]
    public void whenGetMaskedWord_empty_thenReturnEmpty()
    {
        Assert.That(_service.GetMaskedWord(""), Is.EqualTo(string.Empty));
    }
}
