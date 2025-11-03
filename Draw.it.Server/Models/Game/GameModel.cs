using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public string RoomId { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.InGame;
    public int TotalRounds { get; set; }
    public int CurrentRound { get; set; } = 0;
    public long CurrentDrawerId { get; set; }
    public List<long> TurnOrder { get; set; } = new List<long>();
    public int CurrentTurnIndex { get; set; } = 0;
    public int TurnDuration { get; set; }
    public int RemainingSeconds { get; set; }
    public string WordToDraw { get; set; } = string.Empty;
    public List<string> GuessedPlayersIds { get; set; } = new List<string>();
}