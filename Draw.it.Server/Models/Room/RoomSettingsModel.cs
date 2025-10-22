namespace Draw.it.Server.Models.Room
{
    public class RoomSettingsModel : IEquatable<RoomSettingsModel>
    {
        public string RoomName { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public int DrawingTime { get; set; } = 60;
        public int NumberOfRounds { get; set; } = 3;

        public bool Equals(RoomSettingsModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return RoomName == other.RoomName
                   && CategoryId == other.CategoryId
                   && DrawingTime == other.DrawingTime
                   && NumberOfRounds == other.NumberOfRounds;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RoomSettingsModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RoomName, CategoryId, DrawingTime, NumberOfRounds);
        }
    }
}
