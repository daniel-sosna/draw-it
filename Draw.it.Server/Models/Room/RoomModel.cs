using Draw.it.Server.Enums;

namespace Draw.it.Server.Models.Room
{
    public class RoomModel
    {
        public required string Id { get; set; }
        public required long HostId { get; set; }
        public RoomSettingsModel Settings { get; set; } = new RoomSettingsModel();
        public List<long> PlayerIds { get; set; } = new List<long>();
        public RoomStatus Status { get; set; } = RoomStatus.Lobby;
    }
}
