using Draw.it.Server.Data;
using Draw.it.Server.Enums;
using Draw.it.Server.Models.Room;
using Draw.it.Server.Models.User;
using Draw.it.Server.Repositories.User;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace draw_it.Tests.Repositories.User;

[TestFixture]
public class DbUserRepositoryTest
{
    private PostgreSqlContainer _pgContainer;
    private DbContextOptions<ApplicationDbContext> _dbOptions;
    private ApplicationDbContext _context;
    private DbUserRepository _repository;

    private const long UserId = 1;
    private const string Username = "TEST_USER";
    private const string RoomId = "ROOM_1";

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

        using (var migrateCtx = new ApplicationDbContext(_dbOptions))
        {
            await migrateCtx.Database.EnsureCreatedAsync();
            await migrateCtx.Database.MigrateAsync();
        }
    }

    [SetUp]
    public async Task Setup()
    {
        _context = new ApplicationDbContext(_dbOptions);
        _repository = new DbUserRepository(_context);

        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users RESTART IDENTITY CASCADE;");
    }

    [Test]
    public void whenSave_andNewUser_thenInserted()
    {
        var user = CreateUser(UserId, Username);

        _repository.Save(user);

        var fromDb = _repository.FindById(UserId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo(Username));
        Assert.That(fromDb.IsConnected, Is.False);
    }

    [Test]
    public void whenSave_andIdZero_thenInsertedWithGeneratedId()
    {
        var user = CreateUser(0, Username);

        _repository.Save(user);

        Assert.That(user.Id, Is.GreaterThan(0));

        var fromDb = _repository.FindById(user.Id);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo(Username));
    }

    [Test]
    public void whenSave_andUpdateExisting_thenTrackedEntityUpdated()
    {
        var user = CreateUser(UserId, Username);
        _repository.Save(user);

        user.Name = "UPDATED";
        user.IsConnected = true;

        _repository.Save(user);

        var fromDb = _repository.FindById(UserId);

        Assert.That(fromDb!.Name, Is.EqualTo("UPDATED"));
        Assert.That(fromDb.IsConnected, Is.True);
    }

    [Test]
    public void whenSave_andExistingButNotTracked_thenExistingRowUpdated()
    {
        var user = CreateUser(UserId, Username);
        _repository.Save(user);

        _context.ChangeTracker.Clear();

        var updated = CreateUser(UserId, "UPDATED");
        updated.IsConnected = true;

        _repository.Save(updated);

        var fromDb = _repository.FindById(UserId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo("UPDATED"));
        Assert.That(fromDb.IsConnected, Is.True);
    }

    [Test]
    public void whenDeleteById_thenDeletedAndReturnsTrue()
    {
        var user = CreateUser(UserId, Username);
        _repository.Save(user);

        var result = _repository.DeleteById(UserId);

        Assert.That(result, Is.True);
        Assert.That(_repository.FindById(UserId), Is.Null);
    }

    [Test]
    public void whenDeleteById_andDoesNotExist_thenReturnFalse()
    {
        var result = _repository.DeleteById(999);
        Assert.That(result, Is.False);
    }

    [Test]
    public void whenFindById_exists_thenReturnUser()
    {
        var user = CreateUser(UserId, Username);
        _repository.Save(user);

        var fromDb = _repository.FindById(UserId);

        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo(Username));
    }

    [Test]
    public void whenFindById_notExists_thenReturnNull()
    {
        var result = _repository.FindById(123);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void whenGetAll_thenReturnsAllUsers()
    {
        _repository.Save(CreateUser(1, "A"));
        _repository.Save(CreateUser(2, "B"));

        var all = _repository.GetAll().ToList();

        Assert.That(all.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(all.Any(u => u.Id == 1), Is.True);
        Assert.That(all.Any(u => u.Id == 2), Is.True);
    }

    [Test]
    public void whenGetNextId_thenReturnsIncrementedId()
    {
        Assert.That(_repository.GetNextId(), Is.EqualTo(1));

        _repository.Save(CreateUser(1, "A"));

        Assert.That(_repository.GetNextId(), Is.EqualTo(2));
    }

    [Test]
    public async Task whenFindByRoomId_thenReturnUsersAssignedToRoom()
    {
        await InsertRoom(RoomId);
        var u1 = CreateUser(1, "A", RoomId);
        var u2 = CreateUser(2, "B", RoomId);
        var u3 = CreateUser(3, "C"); // different room

        _repository.Save(u1);
        _repository.Save(u2);
        _repository.Save(u3);

        var list = _repository.FindByRoomId(RoomId).ToList();

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.Any(u => u.Name == "A"), Is.True);
        Assert.That(list.Any(u => u.Name == "B"), Is.True);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _pgContainer.DisposeAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
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

    private async Task InsertRoom(string roomId)
    {
        using var roomCtx = new ApplicationDbContext(_dbOptions);
        roomCtx.Rooms.Add(new RoomModel
        {
            Id = roomId,
            HostId = 1,
            Status = RoomStatus.InLobby,
            Settings = new RoomSettingsModel
            {
                DrawingTime = 30,
                CategoryId = 1,
                NumberOfRounds = 3,
                RoomName = "Test Room"
            }
        });

        await roomCtx.SaveChangesAsync();
    }
}
