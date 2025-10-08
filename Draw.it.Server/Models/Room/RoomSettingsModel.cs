namespace Draw.it.Server.Models.Room
{
    public class RoomSettingsModel
    {
        public string RoomName { get; set; } = string.Empty;
        public string[] Categories { get; set; } = Array.Empty<string>();
        public string[] CustomWords { get; set; } = Array.Empty<string>();
        public int DrawingTime { get; set; } = 60;
        public int NumberOfRounds { get; set; } = 3;
    }
}
