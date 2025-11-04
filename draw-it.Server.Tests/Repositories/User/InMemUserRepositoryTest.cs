using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.User;

namespace draw_it.Tests.Repositories.User;

public class InMemUserRepositoryTest
{
    private const string Name = "TEST_NAME";
    private const string AnotherName = "ANOTHER_TEST_NAME";
    private const string RoomId = "TEST_ROOM_ID";
    private const string AnotherRoomId = "ANOTHER_ROOM_ID";

    private InMemUserRepository? _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new InMemUserRepository();
    }

    [Test]
    public void whenGetNextIdCalledMultipleTimes_thenIdsAreSequential()
    {
        long id1 = _repository!.GetNextId();
        long id2 = _repository.GetNextId();
        long id3 = _repository.GetNextId();

        Assert.That(id1, Is.EqualTo(0));
        Assert.That(id2, Is.EqualTo(1));
        Assert.That(id3, Is.EqualTo(2));
    }

    [Test]
    public void whenSaveUserCreatedWithGetNextId_thenUserCanBeFoundById()
    {
        long id = _repository!.GetNextId();
        UserModel user = CreateUser(id, Name);

        _repository.Save(user);

        UserModel? result = _repository.FindById(id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(id));
        Assert.That(result.Name, Is.EqualTo(Name));
    }

    [Test]
    public void whenSaveUserWithoutUsingGetNextId_thenThrowInvalidOperationException()
    {
        // _nextId is still 0 at this point
        UserModel user = new UserModel
        {
            Id = 0,
            Name = Name
        };

        Assert.Throws<InvalidOperationException>(() => _repository!.Save(user));

        // nothing should be stored
        Assert.That(_repository.FindById(0), Is.Null);
    }

    [Test]
    public void whenDeleteExistingUser_thenReturnsTrueAndRemovesUser()
    {
        long id = _repository!.GetNextId();
        UserModel user = CreateUser(id, Name);

        _repository.Save(user);

        bool deleted = _repository.DeleteById(id);

        Assert.That(deleted, Is.True);
        Assert.That(_repository.FindById(id), Is.Null);
    }

    [Test]
    public void whenDeleteNonExistingUser_thenReturnsFalse()
    {
        bool deleted = _repository!.DeleteById(999);

        Assert.That(deleted, Is.False);
    }

    [Test]
    public void whenFindById_andUserDoesNotExist_thenReturnsNull()
    {
        UserModel? result = _repository!.FindById(123);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void whenGetAllUsers_thenReturnAllSavedUsers()
    {
        long id1 = _repository!.GetNextId();
        long id2 = _repository.GetNextId();

        UserModel user1 = CreateUser(id1, Name);
        UserModel user2 = CreateUser(id2, AnotherName);

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
        long id1 = _repository!.GetNextId();
        long id2 = _repository.GetNextId();
        long id3 = _repository.GetNextId();

        UserModel userInRoom1 = CreateUser(id1, Name, RoomId);
        UserModel userInRoom1Second = CreateUser(id2, AnotherName, RoomId);
        UserModel userInAnotherRoom = CreateUser(id3, "THIRD", AnotherRoomId);

        _repository.Save(userInRoom1);
        _repository.Save(userInRoom1Second);
        _repository.Save(userInAnotherRoom);

        var result = _repository.FindByRoomId(RoomId).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Contain(userInRoom1));
        Assert.That(result, Does.Contain(userInRoom1Second));
        Assert.That(result, Does.Not.Contain(userInAnotherRoom));
    }

    private UserModel CreateUser(long id, string name, string? roomId = null)
    {
        return new UserModel
        {
            Id = id,
            Name = name,
            RoomId = roomId,
            IsConnected = false,
            IsReady = false
        };
    }
}
