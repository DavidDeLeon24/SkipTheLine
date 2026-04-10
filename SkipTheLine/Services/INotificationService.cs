using SkipTheLine.Models;

namespace SkipTheLine.Services
{
    // Interface for sending notifications (email and SMS)
    public interface INotificationService
    {
        // Send confirmation email when reservation is created
        Task SendConfirmationEmailAsync(Reservation reservation, User user, Restaurant restaurant);

        // Send reminder email the day before reservation
        Task SendReminderEmailAsync(Reservation reservation, User user, Restaurant restaurant);

        // Send cancellation email when reservation is cancelled
        Task SendCancellationEmailAsync(Reservation reservation, User user, Restaurant restaurant);

        // Send confirmation SMS when reservation is created
        Task SendConfirmationSmsAsync(Reservation reservation, User user, Restaurant restaurant);

        // Send reminder SMS the day before reservation
        Task SendReminderSmsAsync(Reservation reservation, User user, Restaurant restaurant);

        // Notify restaurant owner when new reservation is made
        Task SendOwnerNotificationAsync(Reservation reservation, Restaurant restaurant);
    }
}