using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SkipTheLine.DTOs;
using SkipTheLine.Models;

namespace SkipTheLine.Controllers
{
    [Route("api/[controller]")]  // API endpoint: api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Dependency injection for Identity services
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // POST: api/auth/register - Create new user account
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Validate input model
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists" });

            // Create new user object
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber ?? string.Empty,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };

            // Save user to database with hashed password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign role to user (Customer or Restaurant)
                var roleName = model.Role.ToString();
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                await _userManager.AddToRoleAsync(user, roleName);

                return Ok(new { message = "User registered successfully", userId = user.Id });
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // POST: api/auth/login - Authenticate user and return JWT token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                // Generate JWT token for authenticated user
                var token = await GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    user = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber ?? string.Empty,
                        DietaryPreferences = user.DietaryPreferences,
                        Role = user.Role
                    }
                });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Generate JWT token for authenticated user
        private async Task<string> GenerateJwtToken(User user)
        {
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create claims (user information embedded in token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Get JWT settings from configuration
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKeyString = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(secretKeyString))
            {
                secretKeyString = "DefaultSecretKeyForDevelopmentOnly12345";
            }

            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(jwtSettings["ExpirationDays"] ?? "7"));

            // Build and return JWT token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "SkipTheLineAPI",
                audience: jwtSettings["Audience"] ?? "SkipTheLineClient",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}