namespace Draw.it.Server.Models.Session;

public class SessionModel
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
