namespace Draw.it.Server.Models
{
    public class Room
    {
        public string Id { get; set; } = string.Empty;
        public RoomSettings Settings { get; set; } = new RoomSettings();
        public List<UserRec> Players { get; set; } = new List<UserRec>();
    }
}
