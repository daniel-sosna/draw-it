using Microsoft.Extensions.Options;

namespace Draw.it.Server.Integrations.Gemini;

public class GeminiClient : IGeminiClient
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(IOptions<GeminiOptions> options, ILogger<GeminiClient> logger)
    {
        _apiKey = options.Value.ApiKey;
        _logger = logger;
    }

    public string GuessImage()
    {
        _logger.LogInformation("API KEY {}", _apiKey);
        return "";
    }
}