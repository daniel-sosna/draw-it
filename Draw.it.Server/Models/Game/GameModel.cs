using System.Collections.Concurrent;
using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public required string RoomId { get; set; }
    public int CurrentRound { get; set; } = 0;
    public long CurrentDrawerId { get; set; }
    public string WordToDraw { get; set; } = string.Empty;
    public int CurrentTurnIndex { get; set; } = 0;
    public ConcurrentDictionary<long, bool> GuessedPlayers { get; set; } = new ConcurrentDictionary<long, bool>();
}