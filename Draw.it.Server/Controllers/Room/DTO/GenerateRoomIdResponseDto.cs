using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Controllers.Room.DTO;

public record GenerateRoomIdResponseDto(string RoomId);

public record CreateRoomRequestDto(RoomSettingsModel Settings, long HostUserId);
public record JoinRoomRequestDto(long UserId, bool IsHost);
public record SetReadyStatusRequestDto(bool IsReady);