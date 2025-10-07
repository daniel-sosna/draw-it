using Draw.it.Server.Models.User;

namespace Draw.it.Server.Controllers.Auth.DTO;

public record AuthMeResponseDto(string Name, string? RoomId)
{
    public AuthMeResponseDto(UserModel user) : this(user.Name, user.RoomId) { }
};