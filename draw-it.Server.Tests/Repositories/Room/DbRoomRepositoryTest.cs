using Draw.it.Server.Data;
using Draw.it.Server.Enums;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Repositories.Room;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace draw_it.Tests.Repositories.Room;

[TestFixture]
public class DbRoomRepositoryTest
{
    private PostgreSqlContainer _pgContainer;
    private DbContextOptions<ApplicationDbContext> _dbOptions = null!;
    private ApplicationDbContext _context;
    private DbRoomRepository _repository = null!;

    private const string RoomId = "ROOM_1";
    private const long HostId = 1;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _pgContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithCleanUp(true)
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        await _pgContainer.StartAsync();

        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_pgContainer.GetConnectionString())
            .Options;

        // Apply migrations (if your ApplicationDbContext has migrations)
        using (var migrateContext = new ApplicationDbContext(_dbOptions))
        {
            await migrateContext.Database.EnsureCreatedAsync();
            await migrateContext.Database.MigrateAsync();
        }
    }

    [SetUp]
    public async Task Setup()
    {
        _context = new ApplicationDbContext(_dbOptions);
        _repository = new DbRoomRepository(_context);
        
        // Clear db for every test
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE rooms RESTART IDENTITY CASCADE;");
    }

    [Test]
    public void whenSave_andNewEntity_thenInserted()
    {
        var room = createRoom(RoomId, HostId);

        _repository.Save(room);

        var fromDb = _repository.FindById(RoomId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.HostId, Is.EqualTo(HostId));
        Assert.That(fromDb.Status, Is.EqualTo(RoomStatus.InLobby));
    }

    [Test]
    public void whenSave_andUpdate_thenUpdated()
    {
        var room = createRoom(RoomId, HostId);
        _repository.Save(room);

        room.HostId = 99;
        room.Status = RoomStatus.InGame;

        _repository.Save(room);

        var fromDb = _repository.FindById(RoomId);

        Assert.That(fromDb!.HostId, Is.EqualTo(99));
        Assert.That(fromDb.Status, Is.EqualTo(RoomStatus.InGame));
    }

    [Test]
    public void whenDeleteById_thenDeletedAndReturnsTrue()
    {
        var room = createRoom(RoomId, HostId);
        _repository.Save(room);

        var result = _repository.DeleteById(RoomId);

        Assert.That(result, Is.True);
        Assert.That(_repository.FindById(RoomId), Is.Null);
    }

    [Test]
    public void whenDeleteById_andNotExists_thenReturnFalse()
    {
        var result = _repository.DeleteById("UNKNOWN");
        Assert.That(result, Is.False);
    }

    [Test]
    public void whenFindById_andExists_thenReturnEntity()
    {
        var room = createRoom(RoomId, HostId);
        _repository.Save(room);

        var fromDb = _repository.FindById(RoomId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.HostId, Is.EqualTo(HostId));
    }

    [Test]
    public void whenExistsById_thenReturnTrueOrFalse()
    {
        _repository.Save(createRoom(RoomId, HostId));

        Assert.That(_repository.ExistsById(RoomId), Is.True);
        Assert.That(_repository.ExistsById("NOPE"), Is.False);
    }

    [Test]
    public void whenGetAll_thenReturnsAllRooms()
    {
        _repository.Save(createRoom("R1", HostId));
        _repository.Save(createRoom("R2", HostId));

        var all = _repository.GetAll().ToList();

        Assert.That(all.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(all.Any(r => r.Id == "R1"), Is.True);
        Assert.That(all.Any(r => r.Id == "R2"), Is.True);
    }
    
    // Runs once for all tests
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _pgContainer.DisposeAsync();
    }
    
    // Runs after every test
    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    private RoomModel createRoom(string id, long hostId)
    {
        var roomSettingsModel = new RoomSettingsModel
        {
            DrawingTime = 30,
            CategoryId = 1,
            NumberOfRounds = 3,
            RoomName = "ROOM_NAME"
        };

        return new RoomModel
        {
            HostId = hostId,
            Id = id,
            Status = RoomStatus.InLobby,
            Settings = roomSettingsModel
        };
    }
}
