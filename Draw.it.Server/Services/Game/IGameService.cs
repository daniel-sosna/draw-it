using Draw.it.Server.Models.Game;

namespace Draw.it.Server.Services.Game;

public interface IGameService
{
    void CreateGame(string roomId);
    void DeleteGame(string roomId);
    GameModel GetGame(string roomId);
    long GetCurrentDrawerId(string roomId);
    void SetNextDrawer(GameModel session);
}