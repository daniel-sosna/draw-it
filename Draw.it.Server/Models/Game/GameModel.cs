using Draw.it.Server.Hubs.DTO;

using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public required string RoomId { get; set; }
    public required int PlayerCount { get; set; }
    public HashSet<long> ConnectedPlayersIds { get; set; } = [];
    public int CurrentRound { get; set; } = 1;
    public int CurrentTurnIndex { get; set; } = 0;
    public required long CurrentDrawerId { get; set; }
    public required string WordToDraw { get; set; }
    public List<long> GuessedPlayersIds { get; set; } = [];
    public Dictionary<long, int> CorrectGuesses { get; set; } = [];
    public Dictionary<long, int> RoundScores { get; set; } = [];
    public Dictionary<long, int> TotalScores { get; set; } = [];
    public GamePhase CurrentPhase { get; set; } = GamePhase.DrawingPhase;
    public List<StrokeDto> CanvasStrokes { get; set; } = new();

}