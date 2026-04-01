using SkipTheLine.Enums;

namespace SkipTheLine.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public int TableId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public ReservationStatus Status { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual Table Table { get; set; } = null!;
    }
}