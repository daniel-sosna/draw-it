namespace Draw.it.Server.Models.Game;

public class GameModel
{
    public required string RoomId { get; set; }
    public required int PlayerCount { get; set; }
    public int CurrentRound { get; set; } = 1;
    public int CurrentTurnIndex { get; set; } = 0;
    public required long CurrentDrawerId { get; set; }
    public required string WordToDraw { get; set; }
    public List<long> GuessedPlayersIds { get; set; } = new List<long>();
    public Dictionary<long, int> CorrectGuesses { get; set; } = new Dictionary<long, int>();
    public Dictionary<long, int> RoundScores { get; set; } = new Dictionary<long, int>();
    public Dictionary<long, int> TotalScores { get; set; } = new Dictionary<long, int>();
}