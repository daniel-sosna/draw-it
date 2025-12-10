using Draw.it.Server.Models.Room;

namespace Draw.it.Server.Hubs.DTO;

public record SettingsDto(string RoomName, long CategoryName, int DrawingTime, int NumberOfRounds, bool HasAiPlayer)
{
    public SettingsDto(RoomSettingsModel settings) : this(settings.RoomName, settings.CategoryId, settings.DrawingTime, settings.NumberOfRounds, settings.HasAiPlayer) { }
};