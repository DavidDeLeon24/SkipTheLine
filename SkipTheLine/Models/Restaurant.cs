using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SkipTheLine.Models
{
    // Represents a restaurant in the system
    // Stores all business information and operating details
    public class Restaurant
    {
        // Primary key - unique identifier for each restaurant
        public int Id { get; set; }

        // Basic Information
        public string Name { get; set; } = string.Empty;           // Restaurant name (e.g., "Smile Bakery & Cafe")
        public string Cuisine { get; set; } = string.Empty;        // Food type (e.g., "Italian", "Bakery")

        // Address Information
        public string Address { get; set; } = string.Empty;        // Street address (e.g., "Unit 3 7464 50 Avenue")
        public string City { get; set; } = string.Empty;           // City name (e.g., "Red Deer")
        public string? Province { get; set; }                      // Province/State (e.g., "Alberta")
        public string? PostalCode { get; set; }                    // Zip/Postal code (e.g., "T4P 1X7")
        public string? Country { get; set; }                       // Country name (e.g., "Canada")

        // Contact Information
        public string PhoneNumber { get; set; } = string.Empty;    // Contact phone (e.g., "(825) 706-1388")
        public string Email { get; set; } = string.Empty;          // Contact email
        public string? Website { get; set; }                       // Restaurant website URL

        // Marketing/Description
        public string? Description { get; set; }                   // Restaurant description/blurb
        public string? ImageUrl { get; set; }                      // Photo URL for display

        // Operating Hours
        public TimeSpan OpeningTime { get; set; }                  // When they open (e.g., 08:00)
        public TimeSpan ClosingTime { get; set; }                  // When they close (e.g., 20:00)

        // Capacity Rules
        public int MaxPartySize { get; set; }                      // Largest group they can seat

        // Ownership
        public string OwnerId { get; set; } = string.Empty;        // User ID of the restaurant owner

        // Timestamps
        public DateTime CreatedAt { get; set; }                    // When restaurant was added to system
        public DateTime? UpdatedAt { get; set; }                   // When info was last updated

        // Ratings (from customer reviews)
        public double? Rating { get; set; }                        // Average rating (0-5 stars)
        public int? TotalReviews { get; set; }                     // Number of customer reviews

        // Navigation properties - link to related entities

        // The user who owns this restaurant (linked via OwnerId)
        public virtual User Owner { get; set; } = null!;

        // All tables in this restaurant (one-to-many relationship)
        public virtual ICollection<Table> Tables { get; set; } = new List<Table>();

        // All reservations made at this restaurant (one-to-many relationship)
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}