using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace Draw.it.Server.Integrations.Gemini;

public class GeminiClient : IGeminiClient
{
    private const string Prompt = """
                                  You are an expert hand drawn picture analyzer and you are playing a picture guessing game similar to Skribbl.io. 
                                  
                                  You are given a single hand-drawn image made by a human player. The drawing may be rough, incomplete, or cartoonish, but it usually represents a common word, object, animal, action, or simple concept.
                                  
                                  Your task:
                                  - Look at the drawing.
                                  - Decide what the player is most likely trying to draw.
                                  - Answer with a concrete guess.
                                  
                                  Output rules (VERY IMPORTANT):
                                  - Respond with ONE to THREE words only.
                                  - Prefer a SINGLE everyday English noun if possible (e.g., “cat”, “house”, “airplane”).
                                  - If multiple guesses are needed, return only the most likely one.
                                  - Do NOT write full sentences.
                                  - Do NOT explain your reasoning.
                                  - Do NOT include any extra text, punctuation, quotes, numbering, or emojis.
                                  
                                  If you are unsure, make your best reasonable guess based on what the drawing most resembles, instead of saying you are unsure.
                                  """;
    
    private readonly string _url;
    private readonly ILogger<GeminiClient> _logger;
    private readonly HttpClient _httpClient;

    public GeminiClient(IOptions<GeminiOptions> options, ILogger<GeminiClient> logger, HttpClient httpClient)
    {
        _url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={options.Value.ApiKey}";
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> GuessImage(string imageBase64, string mimeType)
    {
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = Prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mimeType,
                                data = imageBase64
                            }
                        }
                    }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(_url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorMsg = $"Gemini API Error: {response.StatusCode} - {errorContent}";
            _logger.LogWarning(errorMsg);
            return string.Empty;
        }

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonNode>();

        var answer = jsonResponse?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
        return answer ?? string.Empty;
    }
}