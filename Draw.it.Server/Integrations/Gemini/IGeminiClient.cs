namespace Draw.it.Server.Integrations.Gemini;

public interface IGeminiClient
{
    Task<string> GuessImage(string imageBase64, string mimeType);
}