using Draw.it.Server.Models.Game;

namespace Draw.it.Server.Services.Game;

public interface IGameService
{
    void CreateGame(string roomId);
    void DeleteGame(string roomId);
    GameModel GetGame(string roomId);
    long GetDrawerId(string roomId);
    void AddGuessedPlayer(string roomId, long userId, out bool turnAdvanced, out bool gameEnded);
    string GetMaskedWord(string word);
    string GetRandomWord(long categoryId);
}