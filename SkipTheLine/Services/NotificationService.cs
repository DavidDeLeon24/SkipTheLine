using System.Net;
using System.Net.Mail;
using SkipTheLine.Models;

namespace SkipTheLine.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            _logger.LogInformation("=== SendConfirmationEmailAsync START ===");

            if (user == null)
            {
                _logger.LogError("User is null!");
                return;
            }

            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError($"User email is null or empty for user: {user.Id}");
                return;
            }

            // Format time safely - FIXED
            string formattedTime = "Time not available";
            try
            {
                formattedTime = reservation.ReservationTime.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting time");
            }

            var subject = $"Reservation Confirmed - {restaurant.Name}";
            var body = $@"
                <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #ff6b35; color: white; padding: 20px; text-align: center;'>
                        <h1>Reservation Confirmed! 🎉</h1>
                    </div>
                    <div style='padding: 20px; background: #f9f9f9;'>
                        <h2>Hello {user.FirstName}!</h2>
                        <p>Your reservation at <strong>{restaurant.Name}</strong> has been confirmed.</p>
                        <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                        <p><strong>⏰ Time:</strong> {formattedTime}</p>
                        <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                        <p><strong>📍 Location:</strong> {restaurant.Address}, {restaurant.City}</p>
                        <p><strong>📞 Restaurant Phone:</strong> {restaurant.PhoneNumber}</p>
                        {(string.IsNullOrEmpty(reservation.SpecialRequests) ? "" : $"<p><strong>📝 Special Requests:</strong> {reservation.SpecialRequests}</p>")}
                        <hr>
                        <p style='font-size: 12px; color: #666;'>SkipTheLine - Smart Restaurant Reservations</p>
                    </div>
                </div>
            ";

            _logger.LogInformation($"Attempting to send email to: {user.Email}");

            await SendEmailAsync(user.Email, $"{user.FirstName} {user.LastName}", subject, body);

            _logger.LogInformation("=== SendConfirmationEmailAsync END ===");
        }

        public async Task SendReminderEmailAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Cannot send reminder email: User or email is null");
                return;
            }

            // Format time safely
            string formattedTime = "Time not available";
            try
            {
                formattedTime = reservation.ReservationTime.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting time");
            }

            var subject = $"Reminder: Your Reservation at {restaurant.Name} Tomorrow";
            var body = $@"
                <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #ff6b35; color: white; padding: 20px; text-align: center;'>
                        <h1>Reminder: Upcoming Reservation ⏰</h1>
                    </div>
                    <div style='padding: 20px; background: #f9f9f9;'>
                        <h2>Hello {user.FirstName}!</h2>
                        <p>This is a reminder about your reservation at <strong>{restaurant.Name}</strong> tomorrow.</p>
                        <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                        <p><strong>⏰ Time:</strong> {formattedTime}</p>
                        <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                        <p>If you need to cancel, please log into your account.</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(user.Email, $"{user.FirstName} {user.LastName}", subject, body);
        }

        public async Task SendCancellationEmailAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Cannot send cancellation email: User or email is null");
                return;
            }

            // Format time safely
            string formattedTime = "Time not available";
            try
            {
                formattedTime = reservation.ReservationTime.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting time");
            }

            var subject = $"Reservation Cancelled - {restaurant.Name}";
            var body = $@"
                <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #6c757d; color: white; padding: 20px; text-align: center;'>
                        <h1>Reservation Cancelled</h1>
                    </div>
                    <div style='padding: 20px; background: #f9f9f9;'>
                        <h2>Hello {user.FirstName},</h2>
                        <p>Your reservation at <strong>{restaurant.Name}</strong> has been cancelled.</p>
                        <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                        <p><strong>⏰ Time:</strong> {formattedTime}</p>
                        <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                        <p>We hope to see you again soon!</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(user.Email, $"{user.FirstName} {user.LastName}", subject, body);
        }

        public async Task SendOwnerNotificationAsync(Reservation reservation, Restaurant restaurant)
        {
            if (restaurant == null || string.IsNullOrEmpty(restaurant.Email))
            {
                _logger.LogWarning("Cannot send owner notification: Restaurant or email is null");
                return;
            }

            // Format time safely
            string formattedTime = "Time not available";
            try
            {
                formattedTime = reservation.ReservationTime.ToString(@"hh\:mm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting time");
            }

            var subject = $"New Reservation at {restaurant.Name}";
            var body = $@"
                <div style='font-family: Arial; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #28a745; color: white; padding: 20px; text-align: center;'>
                        <h1>New Reservation Received! 🍽️</h1>
                    </div>
                    <div style='padding: 20px; background: #f9f9f9;'>
                        <h2>Hello {restaurant.Name} Team,</h2>
                        <p>A new reservation has been made at your restaurant.</p>
                        <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                        <p><strong>⏰ Time:</strong> {formattedTime}</p>
                        <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                        {(string.IsNullOrEmpty(reservation.SpecialRequests) ? "" : $"<p><strong>📝 Special Requests:</strong> {reservation.SpecialRequests}</p>")}
                        <hr>
                        <p>Login to your dashboard to manage this reservation.</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(restaurant.Email, restaurant.Name, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation($"SendEmailAsync called to: {toEmail}");

                var host = _configuration["Smtp:Host"];
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var username = _configuration["Smtp:Username"];
                var password = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];
                var fromName = _configuration["Smtp:FromName"] ?? "SkipTheLine";

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("SMTP not configured. Email not sent.");
                    return;
                }

                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = username;
                }

                using var client = new SmtpClient(host, port);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var fromAddress = new MailAddress(fromEmail, fromName);
                var toAddress = new MailAddress(toEmail, toName);

                var message = new MailMessage
                {
                    From = fromAddress,
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toAddress);

                _logger.LogInformation($"Sending email via SMTP to {toEmail}...");
                await client.SendMailAsync(message);
                _logger.LogInformation($"✅ Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending email to {toEmail}");
            }
        }

        // SMS methods (not used)
        public Task SendConfirmationSmsAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            return Task.CompletedTask;
        }

        public Task SendReminderSmsAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            return Task.CompletedTask;
        }
    }
}