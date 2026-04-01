using Microsoft.AspNetCore.Identity;
using SkipTheLine.Enums;

namespace SkipTheLine.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? DietaryPreferences { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserRole Role { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}