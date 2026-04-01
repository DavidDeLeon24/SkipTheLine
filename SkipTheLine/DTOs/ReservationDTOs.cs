using SkipTheLine.Enums;

namespace SkipTheLine.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string RestaurantAddress { get; set; } = string.Empty;
        public string RestaurantCity { get; set; } = string.Empty;
        public string RestaurantPhone { get; set; } = string.Empty;
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public ReservationStatus Status { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReservationDto
    {
        public int RestaurantId { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public string? SpecialRequests { get; set; }
    }

    public class UpdateReservationStatusDto
    {
        public ReservationStatus Status { get; set; }
    }

    public class AvailabilitySlotDto
    {
        public string Time { get; set; } = string.Empty;
        public int AvailableTables { get; set; }
        public List<int> TableSizes { get; set; } = new List<int>();
    }
}