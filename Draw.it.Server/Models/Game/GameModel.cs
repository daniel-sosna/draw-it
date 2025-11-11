using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public required string RoomId { get; set; }
    public int CurrentRound { get; set; } = 0;
    public long CurrentDrawerId { get; set; }
    public string WordToDraw { get; set; } = string.Empty;
    public int TotalRounds { get; set; }
    public List<long> TurnOrderIds { get; set; } = new List<long>();
    public int CurrentTurnIndex { get; set; } = 0;
    public List<long> GuessedPlayersIds { get; set; } = new List<long>();
}