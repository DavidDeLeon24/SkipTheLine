using SkipTheLine.Models;

namespace SkipTheLine.Services
{
    public interface INotificationService
    {
        Task SendConfirmationEmailAsync(Reservation reservation, User user, Restaurant restaurant);
        Task SendReminderEmailAsync(Reservation reservation, User user, Restaurant restaurant);
        Task SendCancellationEmailAsync(Reservation reservation, User user, Restaurant restaurant);
        Task SendConfirmationSmsAsync(Reservation reservation, User user, Restaurant restaurant);
        Task SendReminderSmsAsync(Reservation reservation, User user, Restaurant restaurant);
        Task SendOwnerNotificationAsync(Reservation reservation, Restaurant restaurant);
    }
}