namespace Draw.it.Server.Controllers.Room.DTO;

public record JoinRoomRequestDto(long UserId, bool IsHost);