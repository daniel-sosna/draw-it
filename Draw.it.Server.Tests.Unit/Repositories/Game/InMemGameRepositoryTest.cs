using Draw.it.Server.Models.Game;
using Draw.it.Server.Repositories.Game;

namespace Draw.it.Server.Tests.Unit.Repositories.Game;

public class InMemGameRepositoryTest
{
    private InMemGameRepository _repository = null!;
    private const string RoomId = "ROOM_1";

    [SetUp]
    public void Setup()
    {
        _repository = new InMemGameRepository();
    }

    [Test]
    public void whenSave_thenGameIsStoredAndCanBeRetrieved()
    {
        var game = CreateGame();

        _repository.Save(game);

        var result = _repository.FindById(RoomId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.RoomId, Is.EqualTo(RoomId));
        Assert.That(result.WordToDraw, Is.EqualTo("APPLE"));
    }

    [Test]
    public void whenSave_thenExistingGameIsOverwritten()
    {
        var game1 = CreateGame();
        var game2 = CreateGame();
        game2.WordToDraw = "BANANA";

        _repository.Save(game1);
        _repository.Save(game2);  // overwrite

        var result = _repository.FindById(RoomId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.WordToDraw, Is.EqualTo("BANANA"));
    }

    [Test]
    public void whenDeleteExistingGame_thenReturnTrueAndGameRemoved()
    {
        var game = CreateGame();
        _repository.Save(game);

        var result = _repository.DeleteById(RoomId);

        Assert.That(result, Is.True);
        Assert.That(_repository.FindById(RoomId), Is.Null);
    }

    [Test]
    public void whenDeleteNonExistentGame_thenReturnFalse()
    {
        var result = _repository.DeleteById("UNKNOWN");
        Assert.That(result, Is.False);
    }

    [Test]
    public void whenFindById_andGameDoesNotExist_thenReturnNull()
    {
        var result = _repository.FindById("UNKNOWN");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void whenGetAll_thenReturnAllStoredGames()
    {
        var game1 = CreateGame("R1");
        var game2 = CreateGame("R2");

        _repository.Save(game1);
        _repository.Save(game2);

        var allGames = _repository.GetAll().ToList();

        Assert.That(allGames.Count, Is.EqualTo(2));
        Assert.That(allGames.Any(g => g.RoomId == "R1"), Is.True);
        Assert.That(allGames.Any(g => g.RoomId == "R2"), Is.True);
    }

    [Test]
    public void whenRepositoryIsNew_thenGetAllReturnsEmpty()
    {
        var allGames = _repository.GetAll().ToList();
        Assert.That(allGames, Is.Empty);
    }

    private GameModel CreateGame(string roomId = RoomId)
    {
        return new GameModel
        {
            RoomId = roomId,
            PlayerCount = 2,
            CurrentDrawerId = 1,
            CurrentRound = 1,
            CurrentTurnIndex = 0,
            WordToDraw = "APPLE",
            GuessedPlayersIds = new List<long>()
        };
    }
}