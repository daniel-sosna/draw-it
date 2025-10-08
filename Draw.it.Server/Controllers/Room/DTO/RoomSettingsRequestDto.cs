namespace Draw.it.Server.Controllers.Room.DTO;

public record RoomSettingsRequestDto(string RoomName, long CategoryId, int DrawingTime, int NumberOfRounds);