using System.Reflection;
using System.Security.Claims;
using Draw.it.Server.Controllers.User;
using Draw.it.Server.Controllers.User.DTO;
using Draw.it.Server.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace draw_it.Tests.Controllers;

public class UserControllerTest
{
    private const long UserId = 1;
    private const string NewName = "NEW_NAME";

    private UserController _userController;
    private DefaultHttpContext _httpContext;
    private Mock<IUserService> _userService = new();

    [SetUp]
    public void Setup()
    {
        _userService = new Mock<IUserService>();
        _httpContext = new DefaultHttpContext();

        _userController = new UserController(_userService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            }
        };
    }

    [Test]
    public void whenUpdateName_thenServiceCalledAndNoContentReturned()
    {
        SetupAuthenticatedUser();
        var request = new UpdateNameRequestDto(NewName);

        var result = _userController.UpdateName(request);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _userService.Verify(s => s.UpdateName(UserId, NewName), Times.Once);
    }

    [Test]
    public void whenUserControllerClass_thenHasAuthorizeAttribute()
    {
        var attr = typeof(UserController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.That(attr, Is.Not.Null);
    }

    private void SetupAuthenticatedUser()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString())
        });

        _httpContext.User = new ClaimsPrincipal(identity);
    }
}
