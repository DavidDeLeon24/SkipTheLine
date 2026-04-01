using Microsoft.EntityFrameworkCore;
using SkipTheLine.Data;
using SkipTheLine.Enums;
using SkipTheLine.Models;

namespace SkipTheLine.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run every hour
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    await SendDailyReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in reminder service");
                }
            }
        }

        private async Task SendDailyReminders()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var tomorrow = DateTime.Today.AddDays(1);
            var dayAfterTomorrow = DateTime.Today.AddDays(2);

            // Get reservations for tomorrow
            var tomorrowReservations = await context.Reservations
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Where(r => r.ReservationDate.Date == tomorrow &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.Completed &&
                           r.Status != ReservationStatus.NoShow)
                .ToListAsync();

            _logger.LogInformation($"Found {tomorrowReservations.Count} reservations for tomorrow");

            foreach (var reservation in tomorrowReservations)
            {
                try
                {
                    // Send email reminder
                    await notificationService.SendReminderEmailAsync(reservation, reservation.User, reservation.Restaurant);

                    // Send SMS reminder if phone number exists
                    if (!string.IsNullOrEmpty(reservation.User.PhoneNumber))
                    {
                        await notificationService.SendReminderSmsAsync(reservation, reservation.User, reservation.Restaurant);
                    }

                    _logger.LogInformation($"Reminder sent for reservation {reservation.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send reminder for reservation {reservation.Id}");
                }
            }

            // Mark no-shows for yesterday
            var yesterday = DateTime.Today.AddDays(-1);
            var noShowReservations = await context.Reservations
                .Where(r => r.ReservationDate.Date == yesterday &&
                           r.Status == ReservationStatus.Confirmed)
                .ToListAsync();

            foreach (var reservation in noShowReservations)
            {
                reservation.Status = ReservationStatus.NoShow;
                reservation.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation($"Marked reservation {reservation.Id} as no-show");
            }

            if (noShowReservations.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}