using Microsoft.AspNetCore.Identity;
using SkipTheLine.Enums;

namespace SkipTheLine.Models
{
    // Represents a user in the system (customer, restaurant owner, or admin)
    // Inherits from IdentityUser which provides built-in authentication properties
    // (Email, PasswordHash, UserName, etc.)
    public class User : IdentityUser
    {
        // Personal Information
        public string FirstName { get; set; } = string.Empty;     // User's first name (e.g., "John")
        public string LastName { get; set; } = string.Empty;      // User's last name (e.g., "Doe")

        // Customer-specific preferences
        public string? DietaryPreferences { get; set; }           // Food allergies or preferences (e.g., "Vegetarian, no nuts")

        // Account Information
        public DateTime CreatedAt { get; set; }                   // When the account was created
        public UserRole Role { get; set; }                        // Account type: Customer, RestaurantOwner, or Admin

        // Navigation property - links to all reservations made by this user
        // One user can have many reservations (one-to-many relationship)
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}