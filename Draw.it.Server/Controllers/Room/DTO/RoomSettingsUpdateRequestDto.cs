namespace Draw.it.Server.Controllers.Room.DTO;

public record RoomSettingsUpdateRequestDto(
    string RoomName,
    long CategoryId,
    int DrawingTime,
    int NumberOfRounds
);