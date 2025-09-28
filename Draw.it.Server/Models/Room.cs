namespace Draw.it.Server.Models
{
    public class Room
    {
        public string Id { get; set; }
        public RoomSettings Settings { get; set; }
        public List<string> Players { get; set; }
    }
}
