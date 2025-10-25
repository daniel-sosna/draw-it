using Draw.it.Server.Models.User;

namespace Draw.it.Server.Hubs.DTO;

public record PlayerDto(string Name, bool IsHost, bool IsConnected, bool IsReady)
{
    public PlayerDto(UserModel user, bool IsHost) : this(user.Name, IsHost, user.IsConnected, user.IsReady) { }
};