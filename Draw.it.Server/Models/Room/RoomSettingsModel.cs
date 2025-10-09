namespace Draw.it.Server.Models.Room
{
    public class RoomSettingsModel
    {
        public string RoomName { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public int DrawingTime { get; set; } = 60;
        public int NumberOfRounds { get; set; } = 3;
    }
}
