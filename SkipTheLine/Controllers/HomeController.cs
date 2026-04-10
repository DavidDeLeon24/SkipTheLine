using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace SkipTheLine.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // ========== PAGE ROUTES ==========
        // Returns the main home page view
        public IActionResult Index()
        {
            return View();
        }

        // Returns the restaurants listing page
        public IActionResult Restaurants()
        {
            return View();
        }

        // Returns the detailed view of a specific restaurant
        public IActionResult RestaurantDetail()
        {
            return View();
        }

        // Returns the login page
        public IActionResult Login()
        {
            return View();
        }

        // Returns the registration page
        public IActionResult Register()
        {
            return View();
        }

        // Returns the user's reservation history page
        public IActionResult MyReservations()
        {
            return View();
        }

        // Returns the user profile page
        public IActionResult Profile()
        {
            return View();
        }

        // Returns the restaurant owner's dashboard
        public IActionResult OwnerDashboard()
        {
            return View();
        }

        // Returns the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // ========== EMAIL TEST ENDPOINTS ==========
        // Tests SMTP email configuration
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                // Retrieve SMTP settings from configuration
                var host = _configuration["Smtp:Host"];
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var username = _configuration["Smtp:Username"];
                var password = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];
                var fromName = _configuration["Smtp:FromName"] ?? "SkipTheLine";

                // Validate SMTP credentials exist
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Content("❌ ERROR: SMTP credentials not configured in appsettings.json");
                }

                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = username;
                }

                _logger.LogInformation($"Attempting to send email via {host}:{port}");

                // Configure and send email via SMTP client
                using var client = new SmtpClient(host, port);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(username, password);

                var fromAddress = new MailAddress(fromEmail, fromName);
                var toAddress = new MailAddress("johndaviddeleon57@gmail.com", "John David");

                var message = new MailMessage
                {
                    From = fromAddress,
                    Subject = "✅ Test Email from SkipTheLine",
                    Body = $@"
                        <h1>Email Configuration Working!</h1>
                        <p>Test sent at: {DateTime.Now}</p>
                        <p>Your reservation system is ready!</p>
                    ",
                    IsBodyHtml = true
                };
                message.To.Add(toAddress);

                await client.SendMailAsync(message);

                return Content("✅ SUCCESS! Email sent! Check your inbox.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email");
                return Content($"❌ ERROR: {ex.Message}");
            }
        }

        // Tests SendGrid email service configuration
        [HttpGet("test-sendgrid")]
        public async Task<IActionResult> TestSendGrid()
        {
            try
            {
                // Retrieve SendGrid API key from configuration
                var apiKey = _configuration["SendGrid:ApiKey"];

                // Validate API key exists
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Content("❌ ERROR: SendGrid API key not configured");
                }

                // Configure and send email via SendGrid
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("johndaviddeleon57@gmail.com", "SkipTheLine");
                var to = new EmailAddress("johndaviddeleon57@gmail.com", "John David");
                var subject = "✅ SendGrid Test Email";
                var htmlContent = "<h1>Success!</h1><p>SendGrid is working!</p>";
                var plainTextContent = "SendGrid Test Successful!";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                // Check if email was sent successfully
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    return Content($"✅ SUCCESS! Status: {response.StatusCode}");
                }
                else
                {
                    return Content($"❌ ERROR: Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return Content($"❌ ERROR: {ex.Message}");
            }
        }
    }
}