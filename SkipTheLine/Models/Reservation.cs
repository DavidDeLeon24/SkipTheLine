using SkipTheLine.Enums;

namespace SkipTheLine.Models
{
    // Represents a booking/reservation made by a customer
    // Stores all details about a table booking at a restaurant
    public class Reservation
    {
        // Primary key - unique identifier for each reservation
        public int Id { get; set; }

        // ID of the user who made this reservation (foreign key to User table)
        public string UserId { get; set; } = string.Empty;

        // ID of the restaurant being booked (foreign key to Restaurant table)
        public int RestaurantId { get; set; }

        // ID of the specific table assigned (foreign key to Table table)
        public int TableId { get; set; }

        // Date of the reservation (e.g., April 10, 2026)
        public DateTime ReservationDate { get; set; }

        // Time of the reservation (e.g., 6:00 PM)
        public TimeSpan ReservationTime { get; set; }

        // Number of people in the party
        public int PartySize { get; set; }

        // Current status (Confirmed, Cancelled, Seated, etc.)
        public ReservationStatus Status { get; set; }

        // Any special requests from customer (e.g., "Window seat", "Birthday celebration")
        public string? SpecialRequests { get; set; }

        // When this reservation was first created (timestamp)
        public DateTime CreatedAt { get; set; }

        // When this reservation was last modified (nullable if never updated)
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - these link to related entities
        // EF Core uses these to create relationships and load related data

        // The user who made this reservation (linked via UserId)
        public virtual User User { get; set; } = null!;

        // The restaurant being booked (linked via RestaurantId)
        public virtual Restaurant Restaurant { get; set; } = null!;

        // The specific table assigned (linked via TableId)
        public virtual Table Table { get; set; } = null!;
    }
}