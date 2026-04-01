namespace SkipTheLine.Enums
{
    public enum UserRole
    {
        Customer = 1,
        RestaurantOwner = 2,
        Admin = 3
    }

    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        Seated = 3,
        Completed = 4,
        Cancelled = 5,
        NoShow = 6
    }
}