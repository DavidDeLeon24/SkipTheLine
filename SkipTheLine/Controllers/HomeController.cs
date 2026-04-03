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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Restaurants()
        {
            return View();
        }

        public IActionResult RestaurantDetail()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult MyReservations()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult OwnerDashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var host = _configuration["Smtp:Host"];
                var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
                var username = _configuration["Smtp:Username"];
                var password = _configuration["Smtp:Password"];
                var fromEmail = _configuration["Smtp:FromEmail"];
                var fromName = _configuration["Smtp:FromName"] ?? "SkipTheLine";

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return Content("❌ ERROR: SMTP credentials not configured in appsettings.json");
                }

                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = username;
                }

                _logger.LogInformation($"Attempting to send email via {host}:{port}");

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

        [HttpGet("test-sendgrid")]
        public async Task<IActionResult> TestSendGrid()
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    return Content("❌ ERROR: SendGrid API key not configured");
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("johndaviddeleon57@gmail.com", "SkipTheLine");
                var to = new EmailAddress("johndaviddeleon57@gmail.com", "John David");
                var subject = "✅ SendGrid Test Email";
                var htmlContent = "<h1>Success!</h1><p>SendGrid is working!</p>";
                var plainTextContent = "SendGrid Test Successful!";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

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