using System.Reflection;
using System.Security.Claims;
using Draw.it.Server.Controllers.Auth;
using Draw.it.Server.Controllers.Auth.DTO;
using Draw.it.Server.Models.User;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace draw_it.Tests.Controllers;

public class AuthControllerTest
{
    private const long UserId = 1;
    private const string RoomId = "TEST_ROOM_ID";
    private const string UserName = "TEST_NAME";

    private AuthController _authController;
    private DefaultHttpContext _httpContext;
    private Mock<IUserService> _userService = new();
    private Mock<IAuthenticationService> _authService = new();

    [SetUp]
    public void Setup()
    {
        _userService = new Mock<IUserService>();
        _authService = new Mock<IAuthenticationService>();

        var services = new ServiceCollection();
        services.AddSingleton(_authService.Object);
        var provider = services.BuildServiceProvider();

        _httpContext = new DefaultHttpContext
        {
            RequestServices = provider
        };

        _authController = new AuthController(_userService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Test]
    public async Task whenJoin_thenUserCreatedSignedInAndReturnsCreated()
    {
        var request = new AuthJoinRequestDto(UserName);
        var user = new UserModel { Id = UserId, Name = UserName, RoomId = RoomId };

        _userService
            .Setup(s => s.CreateUser(UserName))
            .Returns(user);

        var result = await _authController.Join(request);

        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = (CreatedResult)result;

        Assert.That(createdResult.Location, Is.EqualTo("api/v1/auth/me"));
        Assert.That(createdResult.Value, Is.InstanceOf<AuthMeResponseDto>());

        var dto = (AuthMeResponseDto)createdResult.Value!;
        Assert.That(dto.RoomId, Is.EqualTo(RoomId));
        Assert.That(dto.Name, Is.EqualTo(UserName));

        _userService.Verify(s => s.CreateUser(UserName), Times.Once);
    }

    [Test]
    public void whenMe_thenReturnCurrentUserInfo()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, UserId.ToString()) },
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        _httpContext.User = new ClaimsPrincipal(identity);

        var user = new UserModel { Id = UserId, Name = UserName, RoomId = RoomId };

        _userService
            .Setup(s => s.GetUser(UserId))
            .Returns(user);

        var result = _authController.Me();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;

        Assert.That(okResult.Value, Is.InstanceOf<AuthMeResponseDto>());
        var dto = (AuthMeResponseDto)okResult.Value!;
        Assert.That(dto.RoomId, Is.EqualTo(RoomId));
        Assert.That(dto.Name, Is.EqualTo(UserName));

        _userService.Verify(s => s.GetUser(UserId), Times.Once);
    }

    [Test]
    public async Task whenLogout_thenUserDeletedSignedOutAndReturnsNoContent()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, UserId.ToString()) },
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        _httpContext.User = new ClaimsPrincipal(identity);

        var result = await _authController.Logout();

        Assert.That(result, Is.InstanceOf<NoContentResult>());

        _userService.Verify(s => s.DeleteUser(UserId), Times.Once);

        _authService.Verify(
            a => a.SignOutAsync(
                _httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()
            ),
            Times.Once
        );
    }

    [Test]
    public void whenUnauthorizedAccess_thenReturnUnauthorizedWithMessage()
    {
        var result = _authController.UnauthorizedAccess();

        Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        var unauthorized = (UnauthorizedObjectResult)result;

        Assert.That(unauthorized.Value, Is.EqualTo("Not authenticated"));
    }

    [Test]
    public void whenMe_thenHasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Me));
        Assert.That(method, Is.Not.Null);

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.That(authorizeAttribute, Is.Not.Null);
    }

    [Test]
    public void whenLogout_thenHasAuthorizeAttribute()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Logout));
        Assert.That(method, Is.Not.Null);

        var authorizeAttribute = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.That(authorizeAttribute, Is.Not.Null);
    }
}
