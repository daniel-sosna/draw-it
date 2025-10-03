namespace Draw.it.Server.Models.Room
{
    public class RoomModel
    {
        public string Id { get; set; } = string.Empty;
        public RoomSettingsModel Settings { get; set; } = new RoomSettingsModel();
        public List<string> Players { get; set; } = new List<string>();
    }
}
