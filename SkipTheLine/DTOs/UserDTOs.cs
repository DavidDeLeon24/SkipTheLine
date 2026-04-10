using SkipTheLine.Enums;

namespace SkipTheLine.DTOs
{
    // Data sent from frontend when a user registers a new account
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;        // User's email (used as username)
        public string Password { get; set; } = string.Empty;     // User's password (will be hashed)
        public string FirstName { get; set; } = string.Empty;    // First name (e.g., "John")
        public string LastName { get; set; } = string.Empty;     // Last name (e.g., "Doe")
        public string PhoneNumber { get; set; } = string.Empty;  // Contact phone number
        public UserRole Role { get; set; } = UserRole.Customer;  // Account type: Customer, RestaurantOwner, Admin
    }

    // Data sent from frontend when a user logs in
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;        // User's email address
        public string Password { get; set; } = string.Empty;     // User's password
    }

    // Data sent from backend to frontend about a user
    // Used to display user profile information
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;           // Unique user ID (GUID)
        public string Email { get; set; } = string.Empty;        // User's email address
        public string FirstName { get; set; } = string.Empty;    // First name
        public string LastName { get; set; } = string.Empty;     // Last name
        public string PhoneNumber { get; set; } = string.Empty;  // Phone number
        public string? DietaryPreferences { get; set; }          // Food allergies or preferences (optional)
        public UserRole Role { get; set; }                       // Account type
    }

    // Data sent from frontend when a user updates their profile
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;    // Updated first name
        public string LastName { get; set; } = string.Empty;     // Updated last name
        public string PhoneNumber { get; set; } = string.Empty;  // Updated phone number
        public string? DietaryPreferences { get; set; }          // Updated dietary preferences
    }
}