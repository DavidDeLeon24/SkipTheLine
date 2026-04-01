using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SkipTheLine.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public int MaxPartySize { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double? Rating { get; set; }
        public int? TotalReviews { get; set; }

        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}