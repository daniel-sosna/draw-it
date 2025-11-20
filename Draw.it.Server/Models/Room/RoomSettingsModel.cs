using System.Net;
using Draw.it.Server.Exceptions;

namespace Draw.it.Server.Models.Room
{
    public class RoomSettingsModel : IEquatable<RoomSettingsModel>
    {
        private int _seconds = 60;
        private int _rounds = 2;
        public string RoomName { get; set; } = string.Empty;
        public long CategoryId { get; set; } = 1;

        public int DrawingTime
        {
            get => _seconds;
            set
            {
                if (value < 20 || value > 300)
                    throw new AppException("Drawing time has to be minimum 20 seconds", HttpStatusCode.BadRequest);
                _seconds = value;
            }
        }

        public int NumberOfRounds
        {
            get => _rounds;
            set
            {
                if (value < 1)
                    throw new AppException("Number of rounds has to be at least 1", HttpStatusCode.BadRequest);
                _rounds = value;
            }
        }

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
