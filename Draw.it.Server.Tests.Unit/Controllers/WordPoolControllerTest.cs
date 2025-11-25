using System.Reflection;
using Draw.it.Server.Controllers.WordPool;
using Draw.it.Server.Services.WordPool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace draw_it.Tests.Controllers;

public class WordPoolControllerTest
{
    private const long CategoryId = 42; 
    
    private WordPoolController _controller;
    private Mock<IWordPoolService> _wordPoolService;

    [SetUp]
    public void Setup()
    {
        _wordPoolService = new Mock<IWordPoolService>();
        _controller = new WordPoolController(_wordPoolService.Object);
    }

    [Test]
    public void whenGetCategories_thenServiceCalledAndOkReturned()
    {
        var result = _controller.GetCategories();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _wordPoolService.Verify(s => s.GetAllCategories(), Times.Once);
    }

    [Test]
    public void whenGetWords_thenServiceCalledAndOkReturned()
    {
        var result = _controller.GetWords(CategoryId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _wordPoolService.Verify(s => s.GetWordsByCategoryId(CategoryId), Times.Once);
    }

    [Test]
    public void whenGetRandom_thenServiceCalledAndOkReturned()
    {
        var result = _controller.GetRandom(CategoryId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        _wordPoolService.Verify(s => s.GetRandomWordByCategoryId(CategoryId), Times.Once);
    }

    [Test]
    public void whenWordPoolControllerClass_thenHasAllowAnonymousAttribute()
    {
        var attr = typeof(WordPoolController).GetCustomAttribute<AllowAnonymousAttribute>();
        Assert.That(attr, Is.Not.Null);
    }
}
