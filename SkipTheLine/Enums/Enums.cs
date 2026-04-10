namespace SkipTheLine.Enums
{
    // Defines the different types of user accounts in the system
    // Each user has one of these roles which determines their permissions
    public enum UserRole
    {
        Customer = 1,          // Regular user - can book and manage their own reservations
        RestaurantOwner = 2,   // Can manage restaurants, view all reservations, check-in customers
        Admin = 3              // Full system access - can delete anything, manage all users
    }

    // Defines the possible states of a reservation throughout its lifecycle
    // Tracks where the reservation is in the booking process
    public enum ReservationStatus
    {
        Pending = 1,    // Waiting for confirmation (e.g., payment pending)
        Confirmed = 2,  // Booking is confirmed - customer can show up
        Seated = 3,     // Customer has arrived and been seated at their table
        Completed = 4,  // Meal is done - reservation is finished
        Cancelled = 5,  // Customer cancelled before the reservation time
        NoShow = 6      // Customer didn't show up (automatically set by system)
    }
}