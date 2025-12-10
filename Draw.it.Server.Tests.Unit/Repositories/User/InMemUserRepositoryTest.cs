using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.User;

namespace Draw.it.Server.Tests.Unit.Repositories.User;

public class InMemUserRepositoryTest
{
    private const string Name = "TEST_NAME";
    private const string AnotherName = "ANOTHER_TEST_NAME";
    private const string RoomId = "TEST_ROOM_ID";
    private const string AnotherRoomId = "ANOTHER_ROOM_ID";

    private InMemUserRepository _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new InMemUserRepository();
    }

    [Test]
    public void whenGetNextIdCalledMultipleTimes_thenIdsAreSequential()
    {
        var id1 = _repository.GetNextId();
        var id2 = _repository.GetNextId();
        var id3 = _repository.GetNextId();

        Assert.That(id1, Is.EqualTo(0));
        Assert.That(id2, Is.EqualTo(1));
        Assert.That(id3, Is.EqualTo(2));
    }

    [Test]
    public void whenSaveUserCreatedWithGetNextId_thenUserCanBeFoundById()
    {
        var id = _repository.GetNextId();
        var user = CreateUser(id, Name);

        _repository.Save(user);

        var result = _repository.FindById(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result.Name, Is.EqualTo(Name));
    }

    [Test]
    public void whenSaveUserWithoutUsingGetNextId_thenThrowInvalidOperationException()
    {
        // _nextId is still 0 at this point
        var user = new UserModel
        {
            Id = 0,
            Name = Name
        };

        Assert.Throws<InvalidOperationException>(() => _repository.Save(user));

        // nothing should be stored
        Assert.That(_repository.FindById(0), Is.Null);
    }

    [Test]
    public void whenDeleteExistingUser_thenReturnsTrueAndRemovesUser()
    {
        var id = _repository.GetNextId();
        var user = CreateUser(id, Name);

        _repository.Save(user);

        var deleted = _repository.DeleteById(id);

        Assert.That(deleted, Is.True);
        Assert.That(_repository.FindById(id), Is.Null);
    }

    [Test]
    public void whenDeleteNonExistingUser_thenReturnsFalse()
    {
        var deleted = _repository.DeleteById(999);

        Assert.That(deleted, Is.False);
    }

    [Test]
    public void whenFindById_andUserDoesNotExist_thenReturnsNull()
    {
        var result = _repository.FindById(123);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void whenGetAllUsers_thenReturnAllSavedUsers()
    {
        var id1 = _repository.GetNextId();
        var id2 = _repository.GetNextId();

        var user1 = CreateUser(id1, Name);
        var user2 = CreateUser(id2, AnotherName);

        _repository.Save(user1);
        _repository.Save(user2);

        var users = _repository.GetAll().ToList();

        Assert.That(users.Count, Is.EqualTo(2));
        Assert.That(users, Does.Contain(user1));
        Assert.That(users, Does.Contain(user2));
    }

    [Test]
    public void whenFindByRoomId_thenReturnOnlyUsersInThatRoom()
    {
        var id1 = _repository.GetNextId();
        var id2 = _repository.GetNextId();
        var id3 = _repository.GetNextId();

        var userInRoom1 = CreateUser(id1, Name, RoomId);
        var userInRoom1Second = CreateUser(id2, AnotherName, RoomId);
        var userInAnotherRoom = CreateUser(id3, "THIRD", AnotherRoomId);

        _repository.Save(userInRoom1);
        _repository.Save(userInRoom1Second);
        _repository.Save(userInAnotherRoom);

        var result = _repository.FindByRoomId(RoomId).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Contain(userInRoom1));
        Assert.That(result, Does.Contain(userInRoom1Second));
        Assert.That(result, Does.Not.Contain(userInAnotherRoom));
    }

    [Test]
    public void whenFindAiPlayerByRoom_thenReturnAiPlayer()
    {
        var id1 = _repository.GetNextId();
        var id2 = _repository.GetNextId();
        var id3 = _repository.GetNextId();

        var user1 = CreateUser(id1, Name, RoomId);
        var user2 = CreateUser(id2, AnotherName, RoomId);
        var user3 = CreateUser(id3, "AI", RoomId, true);

        _repository.Save(user1);
        _repository.Save(user2);
        _repository.Save(user3);

        var aiPlayer = _repository.FindAiPlayerByRoomId(RoomId);

        Assert.That(aiPlayer.IsAi);
        Assert.That(aiPlayer.RoomId == RoomId);
    }

    private static UserModel CreateUser(long id, string name, string? roomId = null, bool isAi = false)
    {
        return new UserModel
        {
            Id = id,
            Name = name,
            RoomId = roomId,
            IsConnected = false,
            IsReady = false,
            IsAi = isAi
        };
    }
}
