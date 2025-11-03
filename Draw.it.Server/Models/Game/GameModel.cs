using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public string RoomId { get; set; }
    public int CurrentRound { get; set; } = 0;
    public long CurrentDrawerId { get; set; }
    public string WordToDraw { get; set; } = string.Empty;
}