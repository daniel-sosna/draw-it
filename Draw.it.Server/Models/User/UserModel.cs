namespace Draw.it.Server.Models.User;

public class UserModel
{
    public long Id { get; set; }
    public required string Name { get; set; }

    public bool IsHost { get; set; } = false;

    public bool IsReady { get; set; } = false;
}