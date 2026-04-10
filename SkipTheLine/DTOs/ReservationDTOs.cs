using SkipTheLine.Enums;

namespace SkipTheLine.DTOs
{
    // Data Transfer Object for reservation information
    // Used to send reservation data to the frontend
    public class ReservationDto
    {
        public int Id { get; set; }                     // Unique reservation ID
        public int RestaurantId { get; set; }           // ID of the restaurant
        public string RestaurantName { get; set; } = string.Empty;      // Restaurant name
        public string RestaurantAddress { get; set; } = string.Empty;   // Restaurant address
        public string RestaurantCity { get; set; } = string.Empty;      // Restaurant city
        public string RestaurantPhone { get; set; } = string.Empty;     // Restaurant phone
        public int TableId { get; set; }                // ID of the assigned table
        public int TableNumber { get; set; }            // Table number (e.g., Table #5)
        public DateTime ReservationDate { get; set; }   // Date of reservation (e.g., 2026-04-10)
        public TimeSpan ReservationTime { get; set; }   // Time of reservation (e.g., 18:00)
        public int PartySize { get; set; }              // Number of people
        public ReservationStatus Status { get; set; }   // Status: Confirmed, Cancelled, etc.
        public string? SpecialRequests { get; set; }    // Customer requests (e.g., "Window seat")
        public DateTime CreatedAt { get; set; }         // When reservation was made
    }

    // Data sent from frontend when creating a new reservation
    public class CreateReservationDto
    {
        public int RestaurantId { get; set; }           // Which restaurant to book
        public DateTime ReservationDate { get; set; }   // Desired date
        public TimeSpan ReservationTime { get; set; }   // Desired time
        public int PartySize { get; set; }              // How many people
        public string? SpecialRequests { get; set; }    // Optional special requests
    }

    // Used when owner updates reservation status (e.g., mark as Seated)
    public class UpdateReservationStatusDto
    {
        public ReservationStatus Status { get; set; }   // New status to apply
    }

    // Shows available time slots for a restaurant
    // Used when customer checks table availability
    public class AvailabilitySlotDto
    {
        public string Time { get; set; } = string.Empty;           // Time slot (e.g., "18:00")
        public int AvailableTables { get; set; }                   // Number of free tables
        public List<int> TableSizes { get; set; } = new List<int>(); // What table sizes are free (2,4,6 seats)
    }
}