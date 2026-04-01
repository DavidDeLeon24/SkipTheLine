using System.Net;
using System.Net.Mail;
using SkipTheLine.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

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
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Cannot send email: User or email is null");
                return;
            }

            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("noreply@skiptheline.com", "SkipTheLine");
                    var to = new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}");
                    var subject = $"Reservation Confirmed - {restaurant.Name}";
                    var htmlContent = GetConfirmationEmailBody(reservation, user, restaurant);
                    var plainTextContent = StripHtml(htmlContent);

                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    var response = await client.SendEmailAsync(msg);

                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _logger.LogInformation($"Confirmation email sent to {user.Email}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send email to {user.Email}. Status: {response.StatusCode}");
                    }
                }
                else
                {
                    await SendSmtpEmailAsync(user.Email, "Reservation Confirmed - SkipTheLine", GetConfirmationEmailBody(reservation, user, restaurant));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending confirmation email to {user.Email}");
            }
        }

        public async Task SendReminderEmailAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Cannot send reminder email: User or email is null");
                return;
            }

            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("noreply@skiptheline.com", "SkipTheLine");
                    var to = new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}");
                    var subject = $"Reminder: Your Reservation at {restaurant.Name} Tomorrow";
                    var htmlContent = GetReminderEmailBody(reservation, user, restaurant);
                    var plainTextContent = StripHtml(htmlContent);

                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    await client.SendEmailAsync(msg);

                    _logger.LogInformation($"Reminder email sent to {user.Email}");
                }
                else
                {
                    await SendSmtpEmailAsync(user.Email, "Reminder: Upcoming Reservation - SkipTheLine", GetReminderEmailBody(reservation, user, restaurant));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending reminder email to {user.Email}");
            }
        }

        public async Task SendCancellationEmailAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("Cannot send cancellation email: User or email is null");
                return;
            }

            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress("noreply@skiptheline.com", "SkipTheLine");
                    var to = new EmailAddress(user.Email, $"{user.FirstName} {user.LastName}");
                    var subject = $"Reservation Cancelled - {restaurant.Name}";
                    var htmlContent = GetCancellationEmailBody(reservation, user, restaurant);
                    var plainTextContent = StripHtml(htmlContent);

                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    await client.SendEmailAsync(msg);

                    _logger.LogInformation($"Cancellation email sent to {user.Email}");
                }
                else
                {
                    await SendSmtpEmailAsync(user.Email, "Reservation Cancelled - SkipTheLine", GetCancellationEmailBody(reservation, user, restaurant));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending cancellation email to {user.Email}");
            }
        }

        public async Task SendConfirmationSmsAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogWarning("Cannot send SMS: User or phone number is null");
                return;
            }

            try
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromNumber = _configuration["Twilio:PhoneNumber"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
                {
                    _logger.LogWarning("Twilio not configured properly. SMS not sent.");
                    return;
                }

                TwilioClient.Init(accountSid, authToken);

                var messageBody = $"SkipTheLine: Your reservation at {restaurant.Name} on {reservation.ReservationDate:MMMM dd, yyyy} at {reservation.ReservationTime:hh\\:mm tt} for {reservation.PartySize} people is confirmed. Text STOP to opt out.";

                var message = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(user.PhoneNumber)
                );

                _logger.LogInformation($"Confirmation SMS sent to {user.PhoneNumber}. SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending confirmation SMS to {user.PhoneNumber}");
            }
        }

        public async Task SendReminderSmsAsync(Reservation reservation, User user, Restaurant restaurant)
        {
            if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogWarning("Cannot send reminder SMS: User or phone number is null");
                return;
            }

            try
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromNumber = _configuration["Twilio:PhoneNumber"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
                {
                    _logger.LogWarning("Twilio not configured properly. SMS not sent.");
                    return;
                }

                TwilioClient.Init(accountSid, authToken);

                var messageBody = $"SkipTheLine Reminder: You have a reservation at {restaurant.Name} tomorrow at {reservation.ReservationTime:hh\\:mm tt} for {reservation.PartySize} people. Call {restaurant.PhoneNumber} if you need to cancel.";

                var message = await MessageResource.CreateAsync(
                    body: messageBody,
                    from: new PhoneNumber(fromNumber),
                    to: new PhoneNumber(user.PhoneNumber)
                );

                _logger.LogInformation($"Reminder SMS sent to {user.PhoneNumber}. SID: {message.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending reminder SMS to {user.PhoneNumber}");
            }
        }

        public async Task SendOwnerNotificationAsync(Reservation reservation, Restaurant restaurant)
        {
            if (restaurant == null || string.IsNullOrEmpty(restaurant.Email))
            {
                _logger.LogWarning("Cannot send owner notification: Restaurant or email is null");
                return;
            }

            try
            {
                var ownerEmail = restaurant.Email;
                var subject = $"New Reservation at {restaurant.Name}";
                var body = $@"
                    <h2>New Reservation Received!</h2>
                    <p><strong>Restaurant:</strong> {restaurant.Name}</p>
                    <p><strong>Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                    <p><strong>Time:</strong> {reservation.ReservationTime:hh\\:mm tt}</p>
                    <p><strong>Party Size:</strong> {reservation.PartySize} people</p>
                    <p><strong>Table:</strong> Table #{reservation.TableId}</p>
                    <p><strong>Special Requests:</strong> {reservation.SpecialRequests ?? "None"}</p>
                    <hr />
                    <p>Login to your dashboard to manage this reservation.</p>
                ";

                await SendSmtpEmailAsync(ownerEmail, subject, body);
                _logger.LogInformation($"Owner notification sent to {ownerEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending owner notification to {restaurant.Email}");
            }
        }

        private async Task SendSmtpEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("Cannot send SMTP email: Recipient email is null or empty");
                return;
            }

            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@skiptheline.com";
                var fromName = smtpSettings["FromName"] ?? "SkipTheLine";

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("SMTP not configured properly. Email not sent.");
                    return;
                }

                using var client = new SmtpClient(host, port);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"SMTP email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMTP email to {toEmail}");
            }
        }

        private string GetConfirmationEmailBody(Reservation reservation, User user, Restaurant restaurant)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ff6b35; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #ff6b35; color: white; text-decoration: none; border-radius: 5px; }}
                        .details {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Reservation Confirmed! 🎉</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {user.FirstName}!</h2>
                            <p>Your reservation at <strong>{restaurant.Name}</strong> has been confirmed.</p>
                            
                            <div class='details'>
                                <h3>Reservation Details:</h3>
                                <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                                <p><strong>⏰ Time:</strong> {reservation.ReservationTime:hh\\:mm tt}</p>
                                <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                                <p><strong>📍 Location:</strong> {restaurant.Address}, {restaurant.City}</p>
                                <p><strong>📞 Restaurant Phone:</strong> {restaurant.PhoneNumber}</p>
                                {(string.IsNullOrEmpty(reservation.SpecialRequests) ? "" : $"<p><strong>📝 Special Requests:</strong> {reservation.SpecialRequests}</p>")}
                            </div>
                            
                            <p>To cancel or modify your reservation, please visit your profile page.</p>
                            <p style='text-align: center;'>
                                <a href='https://localhost:5001/Profile' class='button'>View My Reservations</a>
                            </p>
                            <p>We look forward to serving you!</p>
                            <p><strong>SkipTheLine Team</strong></p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GetReminderEmailBody(Reservation reservation, User user, Restaurant restaurant)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ff6b35; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .reminder {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Reminder: Upcoming Reservation ⏰</h1>
                        </div>
                        <div class='content'>
                            <h2>Hi {user.FirstName}!</h2>
                            <p>This is a reminder about your reservation at <strong>{restaurant.Name}</strong> tomorrow.</p>
                            
                            <div class='reminder'>
                                <p><strong>📅 Date:</strong> {reservation.ReservationDate:MMMM dd, yyyy}</p>
                                <p><strong>⏰ Time:</strong> {reservation.ReservationTime:hh\\:mm tt}</p>
                                <p><strong>👥 Party Size:</strong> {reservation.PartySize} people</p>
                            </div>
                            
                            <p>If you need to cancel, please do so at least 2 hours before your reservation time.</p>
                            <p><strong>Restaurant Contact:</strong> {restaurant.PhoneNumber}</p>
                            <p>Enjoy your meal! 🍽️</p>
                        </div>
                        <div class='footer'>
                            <p>Need to cancel? Visit your profile page.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GetCancellationEmailBody(Reservation reservation, User user, Restaurant restaurant)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Reservation Cancelled</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {user.FirstName},</h2>
                            <p>Your reservation at <strong>{restaurant.Name}</strong> has been cancelled.</p>
                            
                            <p><strong>Original Reservation:</strong></p>
                            <p>📅 {reservation.ReservationDate:MMMM dd, yyyy} at {reservation.ReservationTime:hh\\:mm tt} for {reservation.PartySize} people</p>
                            
                            <p>We hope to see you again soon!</p>
                            <p><strong>SkipTheLine Team</strong></p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}