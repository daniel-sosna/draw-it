using Draw.it.Server.Models.Game;

namespace Draw.it.Server.Services.Game;

public interface IGameService
{
    void CreateGame(string roomId);
    void DeleteGame(string roomId);
    GameModel GetGame(string roomId);
    long GetDrawerId(string roomId);
    void SetDrawerId(string roomId, long newDrawerId);
    bool AddGuessedPlayer(string roomId, long userId);
    void ClearGuessedPlayers(string roomId);
    bool AdvanceTurn(string roomId);
    string GetMaskedWord(string word);
    string GetRandomWord(long categoryId);
}