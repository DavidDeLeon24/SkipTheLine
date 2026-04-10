namespace SkipTheLine.Models
{
    // Represents a physical table in a restaurant
    // Each table has a capacity and can be booked for reservations
    public class Table
    {
        // Primary key - unique identifier for each table
        public int Id { get; set; }

        // ID of the restaurant this table belongs to (foreign key to Restaurant table)
        public int RestaurantId { get; set; }

        // Table number for identification (e.g., Table #1, Table #2)
        // Visible to customers so they know where to sit
        public int TableNumber { get; set; }

        // How many people can sit at this table (e.g., 2, 4, 6)
        public int Seats { get; set; }

        // Whether this table is available for booking
        // Owner can disable a table if it's broken or reserved for special events
        public bool IsActive { get; set; } = true;  // Default: true (active)

        // Navigation properties - link to related entities

        // The restaurant this table belongs to (linked via RestaurantId)
        public virtual Restaurant Restaurant { get; set; } = null!;

        // All reservations made for this table (one-to-many relationship)
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}