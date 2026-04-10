namespace SkipTheLine.DTOs
{
    // Data Transfer Object for sending restaurant information to the frontend
    // Used when displaying restaurant details to customers
    public class RestaurantDto
    {
        public int Id { get; set; }                      // Unique restaurant ID
        public string Name { get; set; } = string.Empty; // Restaurant name (e.g., "Smile Bakery & Cafe")
        public string Cuisine { get; set; } = string.Empty; // Food type (e.g., "Italian", "Bakery")
        public string Address { get; set; } = string.Empty; // Street address
        public string City { get; set; } = string.Empty;    // City name
        public string? Province { get; set; }            // Province/State (optional)
        public string? PostalCode { get; set; }          // Zip/Postal code (optional)
        public string? Country { get; set; }             // Country name (optional)
        public string PhoneNumber { get; set; } = string.Empty; // Contact phone
        public string Email { get; set; } = string.Empty;     // Contact email
        public string? Website { get; set; }             // Restaurant website URL (optional)
        public string? Description { get; set; }         // Restaurant description (optional)
        public string? ImageUrl { get; set; }            // Restaurant photo URL (optional)
        public TimeSpan OpeningTime { get; set; }        // Opening hour (e.g., 09:00)
        public TimeSpan ClosingTime { get; set; }        // Closing hour (e.g., 21:00)
        public int MaxPartySize { get; set; }            // Largest group they can seat
        public double? Rating { get; set; }              // Average customer rating (0-5)
        public int? TotalReviews { get; set; }           // Number of customer reviews
    }

    // Data sent from frontend when owner adds a new restaurant
    // Contains all required fields to create a restaurant
    public class CreateRestaurantDto
    {
        public string Name { get; set; } = string.Empty;      // Restaurant name (required)
        public string Cuisine { get; set; } = string.Empty;   // Food type (required)
        public string Address { get; set; } = string.Empty;   // Street address (required)
        public string City { get; set; } = string.Empty;      // City name (required)
        public string? Province { get; set; }                 // Province (optional)
        public string? PostalCode { get; set; }               // Postal code (optional)
        public string? Country { get; set; }                  // Country (optional)
        public string PhoneNumber { get; set; } = string.Empty; // Contact phone (required)
        public string Email { get; set; } = string.Empty;     // Contact email (required)
        public string? Website { get; set; }                  // Website URL (optional)
        public string? Description { get; set; }              // Description (optional)
        public TimeSpan OpeningTime { get; set; }             // Opening hour (required)
        public TimeSpan ClosingTime { get; set; }             // Closing hour (required)
        public int MaxPartySize { get; set; }                 // Max group size (required)
    }

    // Data sent from frontend when owner updates an existing restaurant
    // Similar to CreateRestaurantDto but without ID (ID comes from URL)
    public class UpdateRestaurantDto
    {
        public string Name { get; set; } = string.Empty;      // Updated name
        public string Cuisine { get; set; } = string.Empty;   // Updated cuisine
        public string Address { get; set; } = string.Empty;   // Updated address
        public string City { get; set; } = string.Empty;      // Updated city
        public string? Province { get; set; }                 // Updated province
        public string? PostalCode { get; set; }               // Updated postal code
        public string? Country { get; set; }                  // Updated country
        public string PhoneNumber { get; set; } = string.Empty; // Updated phone
        public string? Website { get; set; }                  // Updated website
        public string? Description { get; set; }              // Updated description
        public TimeSpan OpeningTime { get; set; }             // Updated opening hour
        public TimeSpan ClosingTime { get; set; }             // Updated closing hour
        public int MaxPartySize { get; set; }                 // Updated max party size
    }
}