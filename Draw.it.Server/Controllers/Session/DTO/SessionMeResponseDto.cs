using Draw.it.Server.Models.User;

namespace Draw.it.Server.Controllers.Session.DTO;

public record SessionMeResponseDto(UserModel user, string? roomId);