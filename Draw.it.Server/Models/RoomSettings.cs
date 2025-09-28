namespace Draw.it.Server.Models
{
    public class RoomSettings
    {
        public string RoomName { get; set; }
        public string[] Categories { get; set; }
        public string[] CustomWords { get; set; }
        public int DrawingTime { get; set; }
        public int NumberOfRounds { get; set; }
    }
}
