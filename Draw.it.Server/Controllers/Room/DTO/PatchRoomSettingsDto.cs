namespace Draw.it.Server.Controllers.Room.DTO;

public record class PatchRoomSettingsDto
{
    public string? RoomName { get; set; }
    public int? DrawingTime { get; set; }
    public int? NumberOfRounds { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? CustomWords { get; set; }
}