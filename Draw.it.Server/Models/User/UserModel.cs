namespace Draw.it.Server.Models.User;

public class UserModel
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public string? RoomId { get; set; } // Link to Room
}