namespace Draw.it.Server.Models.Room
{
    public class RoomModel
    {
        public string Id { get; set; } = string.Empty;
        public long HostId { get; set; }
        public RoomSettingsModel Settings { get; set; } = new RoomSettingsModel();
        public List<long> PlayerIds { get; set; } = new List<long>();
    }
}
