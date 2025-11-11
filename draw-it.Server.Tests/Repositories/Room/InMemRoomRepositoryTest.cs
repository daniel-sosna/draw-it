using Draw.it.Server.Models.Room;
using Draw.it.Server.Repositories.Room;
namespace draw_it.Tests.Repositories.Room;

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
        RoomModel room = new RoomModel{ Id = RoomId, HostId = HostId };

        _repository.Save(room);
        RoomModel? result = _repository.FindById(RoomId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(room));
    }

    [Test]
    public void whenSaveRoom_thenExistsByIdReturnsTrue()
    {
        RoomModel room = new RoomModel{ Id = RoomId, HostId = HostId };

        _repository.Save(room);

        bool exists = _repository.ExistsById(RoomId);

        Assert.That(exists, Is.True);
    }

    [Test]
    public void whenRoomNotSaved_thenExistsByIdReturnsFalse()
    {
        bool exists = _repository.ExistsById(RoomId);

        Assert.That(exists, Is.False);
    }

    [Test]
    public void whenDeleteExistingRoom_thenReturnsTrueAndRoomIsRemoved()
    {
        RoomModel room = new RoomModel{ Id = RoomId, HostId = HostId };
        _repository.Save(room);

        bool deleted = _repository.DeleteById(RoomId);

        Assert.That(deleted, Is.True);
        Assert.That(_repository.FindById(RoomId), Is.Null);
        Assert.That(_repository.ExistsById(RoomId), Is.False);
    }

    [Test]
    public void whenDeleteNonExistingRoom_thenReturnsFalse()
    {
        bool deleted = _repository.DeleteById(RoomId);

        Assert.That(deleted, Is.False);
    }

    [Test]
    public void whenMultipleRoomsSaved_thenGetAllReturnsAllRooms()
    {
        RoomModel room1 = new RoomModel{ Id = RoomId, HostId = HostId };
        RoomModel room2 = new RoomModel { Id = AnotherRoomId, HostId = HostId };

        _repository.Save(room1);
        _repository.Save(room2);

        var rooms = _repository.GetAll().ToList();

        Assert.That(rooms.Count, Is.EqualTo(2));
        Assert.That(rooms, Does.Contain(room1));
        Assert.That(rooms, Does.Contain(room2));
    }
    
}
