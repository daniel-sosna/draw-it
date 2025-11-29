using Draw.it.Server.Models.Room;
using Draw.it.Server.Repositories.Room;

namespace Draw.it.Server.Tests.Unit.Repositories.Room;

public class InMemRoomRepositoryTest
{
    private const string RoomId = "TEST_ROOM_ID";
    private const long HostId = 1;
    private const string AnotherRoomId = "ANOTHER_ROOM_ID";

    private InMemRoomRepository _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new InMemRoomRepository();
    }

    [Test]
    public void whenSaveRoom_thenRoomCanBeFoundById()
    {
        var room = new RoomModel{ Id = RoomId, HostId = HostId };

        _repository.Save(room);
        var result = _repository.FindById(RoomId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(room));
    }

    [Test]
    public void whenSaveRoom_thenExistsByIdReturnsTrue()
    {
        var room = new RoomModel{ Id = RoomId, HostId = HostId };

        _repository.Save(room);

        var exists = _repository.ExistsById(RoomId);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void whenRoomNotSaved_thenExistsByIdReturnsFalse()
    {
        var exists = _repository.ExistsById(RoomId);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void whenDeleteExistingRoom_thenReturnsTrueAndRoomIsRemoved()
    {
        var room = new RoomModel{ Id = RoomId, HostId = HostId };
        _repository.Save(room);

        var deleted = _repository.DeleteById(RoomId);

        Assert.That(deleted, Is.True);
        Assert.That(_repository.FindById(RoomId), Is.Null);
        Assert.That(_repository.ExistsById(RoomId), Is.False);
    }

    [Test]
    public void whenDeleteNonExistingRoom_thenReturnsFalse()
    {
        var deleted = _repository.DeleteById(RoomId);

        Assert.That(deleted, Is.False);
    }

    [Test]
    public void whenMultipleRoomsSaved_thenGetAllReturnsAllRooms()
    {
        var room1 = new RoomModel{ Id = RoomId, HostId = HostId };
        var room2 = new RoomModel{ Id = AnotherRoomId, HostId = HostId };

        _repository.Save(room1);
        _repository.Save(room2);

        var rooms = _repository.GetAll().ToList();

        Assert.That(rooms, Has.Count.EqualTo(2));
        Assert.That(rooms, Does.Contain(room1));
        Assert.That(rooms, Does.Contain(room2));
    }
    
}