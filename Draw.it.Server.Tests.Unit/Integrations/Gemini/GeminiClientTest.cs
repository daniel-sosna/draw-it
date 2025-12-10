using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Draw.it.Server.Integrations.Gemini;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Draw.it.Server.Tests.Unit.Integrations.Gemini;

public class GeminiClientTests
{
    private const string ApiKey = "TEST_KEY";
    private const string MimeType = "image/png";
    private const string ImageBase64 = "BASE64_IMAGE";

    private Mock<IOptions<GeminiOptions>> _optionsMock;
    private Mock<ILogger<GeminiClient>> _loggerMock;
    private Mock<HttpMessageHandler> _handlerMock;
    private HttpClient _httpClient;
    private GeminiClient _client;

    [SetUp]
    public void SetUp()
    {
        _optionsMock = new Mock<IOptions<GeminiOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(new GeminiOptions { ApiKey = ApiKey });
        _loggerMock = new Mock<ILogger<GeminiClient>>();
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object);
        _client = new GeminiClient(_optionsMock.Object, _loggerMock.Object, _httpClient);
    }


    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task GuessImage_WhenResponseIsSuccessful_ReturnsAnswerText()
    {
        var json = new JsonObject
        {
            ["candidates"] = new JsonArray
            {
                new JsonObject
                {
                    ["content"] = new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject { ["text"] = "cat" }
                        }
                    }
                }
            }
        }.ToJsonString();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post &&
                    r.RequestUri!.ToString().Contains("gemini-2.5-flash:generateContent") &&
                    r.RequestUri!.Query.Contains($"key={ApiKey}")
                ),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse)
            .Verifiable();

        var result = await _client.GuessImage(ImageBase64, MimeType);

        Assert.That(result, Is.EqualTo("cat"));

        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task GuessImage_WhenResponseIsError_LogsWarningAndReturnsEmptyString()
    {
        const string errorBody = "bad request";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(errorBody, Encoding.UTF8, "text/plain")
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _client.GuessImage(ImageBase64, MimeType);

        Assert.That(result, Is.EqualTo(string.Empty));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Gemini API Error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task GuessImage_WhenResponseHasNoText_ReturnsEmptyString()
    {
        var json = new JsonObject
        {
            ["candidates"] = new JsonArray
            {
                new JsonObject
                {
                    ["content"] = new JsonObject
                    {
                        ["parts"] = new JsonArray
                        {
                            new JsonObject()
                        }
                    }
                }
            }
        }.ToJsonString();

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var result = await _client.GuessImage(ImageBase64, MimeType);

        Assert.That(result, Is.EqualTo(string.Empty));
    }
}
