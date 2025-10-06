namespace Draw.it.Server.Models.Session;

public class SessionModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();  // Short-lived identifier
    public required long UserId { get; set; }   // Link to User
    public string? RoomId { get; set; } // Link to Room
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
