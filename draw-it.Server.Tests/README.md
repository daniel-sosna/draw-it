# How to use/write unit tests

### 1. NUnit, Assert and Moq basics

Attributes:
1. `[Test]` - Marks a method as a test that NUnit should run.
2. `[SetUp]` - Runs before each test in the class.

Assertions:
Used to check if returned value is equal to expected
```c#
Assert.That(result, Is.EqualTo(5));    
Assert.IsTrue(condition);
Assert.Throws<ArgumentException>(() => ...);
```

Mocks:
A mock is a fake object that imitates a real dependency in a test, allowing you to control its behavior and verify how your code interacts with it.

For example, you have this piece of code:
```c#
public class UserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public string GetUserName(long id)
    {
        var user = _repo.GetUser(id);
        return user?.Name ?? "Unknown";
    }
}
```
You don’t want to connect to a real database during a unit test — that would be slow and unreliable.
So you use Moq to fake the repository.

1. Create a mock: 
   ```c#
   var mockRepo = new Mock<IUserRepository>();
   ```
2. Tell the mock what to return:
   ```c#
   mockRepo.Setup(r => r.GetUser(1)).Returns(new User { Id = 1, Name = "Alice" });
   ```
3. Pass the mock into your service: 
    ```c#
    var service = new UserService(mockRepo.Object);
    ```
4. Now calling the service will return your mocked value:
    ```c#
    var result = service.GetUserName(1);
    Assert.That(result, Is.EqualTo("Alice")); // True
    ```

5. You can also check that certain methods were called:
    ```c#
    mockRepo.Verify(r => r.GetUser(1), Times.Once);
    ```
### 2. Use the AAA (Arrange – Act – Assert) principle

The AAA principle consists of 3 phases:
1. Arrange
    This is the setup phase. You prepare everything the test needs: create objects and mocks, define input data, configure mock behavior (Setup)
    Example:
    ```c#
    var mockRepo = new Mock<IUserRepository>();
    mockRepo.Setup(r => r.GetUser(1)).Returns(new User { Id = 1, Name = "Alice" });
    
    var service = new UserService(mockRepo.Object);
    ```

2. Act
    This is the execution phase. You call the method you are testing
    Example:
    ```c#
    var result = service.GetUserName(1);
    ```

3. Assert
    This is the verification phase. You check that the outcome is what you expect: a return value, an exception, a call on a mock, ...
    Example:
    ```c#
    Assert.That(result, Is.EqualTo("Alice"));
    mockRepo.Verify(r => r.GetUser(1), Times.Once);
    ```

### 3. Integration tests

Integration tests verify that multiple components work correctly together — for example: A repository works correctly with a real PostgreSQL database


This project uses the Testcontainers library to automatically spin up a real PostgreSQL instance during tests.

A typical setup looks like this:

```C#
private PostgreSqlContainer _pgContainer;

[OneTimeSetUp]
public async Task OneTimeSetup()
{
    _pgContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .WithImage("postgres:16")
        .Build();

    await _pgContainer.StartAsync();

    // Configure EF Core to use the container
    _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(_pgContainer.GetConnectionString())
        .Options;

    // Apply migrations once
    using var ctx = new ApplicationDbContext(_dbOptions);
    await ctx.Database.MigrateAsync();
}

```

PostgreSQL database itself persists for the entire test run.
Because of this, tests must clean up the database between runs, usually in `[SetUp]`:

```C#
[SetUp]
public async Task Setup()
{
    _context = new ApplicationDbContext(_dbOptions);
    _repository = new DbRoomRepository(_context);

    // Reset state for next test
    await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE rooms, users RESTART IDENTITY CASCADE;");
}
```

Avoid integration tests for:
Business logic that doesn’t depend on external systems
Small pure functions (these should be covered by unit tests)

Integration tests are slower — use them only when needed.

### 4. Run the tests

From the `draw-it.Server.Tests` directory run the following command:
```cmd
dotnet test
```

To run tests with code coverage you have run this command:
```cmd
dotnet test --collect:"XPlat Code Coverage"
```

This will generate a code coverage xml report file in `draw-it.Server.Tests/TestResults/{SOME_UUID}/coverage.cobertura.xml`.
Unfortunately this is not really human-readable. For this `reportgenerator` tool can help. Install it with:

```cmd
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Generate the report with:
```cmd
reportgenerator -reports:TestResults/**/*.xml -targetdir:coveragereport
```

This will create a directory `coveragereport/` where `.html` files will be generated with pretty graphs