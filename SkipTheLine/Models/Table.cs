namespace SkipTheLine.Models
{
    public class Table
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int TableNumber { get; set; }
        public int Seats { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual Restaurant Restaurant { get; set; } = null!;
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}