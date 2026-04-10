using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SkipTheLine.DTOs;
using SkipTheLine.Models;
using SkipTheLine.Data;
using System.Security.Claims;

namespace SkipTheLine.Controllers
{
    [Route("api/[controller]")]  // API endpoint: api/users
    [ApiController]
    [Authorize]  // All endpoints require authentication
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<User> userManager,
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/users/profile - Get current user's profile information
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(_mapper.Map<UserDto>(user));
        }

        // PUT: api/users/profile - Update current user's profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Update user fields
            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.DietaryPreferences = updateDto.DietaryPreferences;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} updated profile");
                return Ok(new { message = "Profile updated successfully", user = _mapper.Map<UserDto>(user) });
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        // GET: api/users/reservations - Get current user's all reservations
        [HttpGet("reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var reservations = await _context.Reservations
                .Include(r => r.Restaurant)  // Include restaurant details
                .Include(r => r.Table)       // Include table details
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }

        // GET: api/users/restaurant/dashboard - Get dashboard for restaurant owners
        [HttpGet("restaurant/dashboard")]
        [Authorize(Roles = "RestaurantOwner,Admin")]  // Only owners and admins can access
        public async Task<ActionResult<object>> GetOwnerDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get all restaurants owned by this user
            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .ToListAsync();

            if (!restaurants.Any())
                return Ok(new { message = "You don't own any restaurants yet" });

            var dashboardData = new List<object>();

            foreach (var restaurant in restaurants)
            {
                var today = DateTime.Today;

                // Get upcoming reservations (today and future, not cancelled/completed)
                var upcomingReservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Table)
                    .Where(r => r.RestaurantId == restaurant.Id &&
                               r.ReservationDate >= today &&
                               r.Status != Enums.ReservationStatus.Cancelled &&
                               r.Status != Enums.ReservationStatus.Completed)
                    .OrderBy(r => r.ReservationDate)
                    .ThenBy(r => r.ReservationTime)
                    .ToListAsync();

                // Get today's reservations only
                var todayReservations = upcomingReservations
                    .Where(r => r.ReservationDate.Date == today)
                    .ToList();

                var totalTables = restaurant.Tables.Count;
                var availableTables = restaurant.Tables.Count(t => t.IsActive);

                dashboardData.Add(new
                {
                    restaurant = _mapper.Map<RestaurantDto>(restaurant),
                    stats = new
                    {
                        totalReservations = upcomingReservations.Count,
                        todayReservations = todayReservations.Count,
                        totalTables = totalTables,
                        availableTables = availableTables
                    },
                    upcomingReservations = _mapper.Map<List<ReservationDto>>(upcomingReservations)
                });
            }

            return Ok(dashboardData);
        }

        // GET: api/users/restaurant/{restaurantId}/reservations - Get reservations for specific restaurant
        [HttpGet("restaurant/{restaurantId}/reservations")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetRestaurantReservations(
            int restaurantId,
            [FromQuery] DateTime? startDate,  // Optional start date filter
            [FromQuery] DateTime? endDate)    // Optional end date filter
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verify restaurant exists
            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            // Verify ownership
            if (restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .Where(r => r.RestaurantId == restaurantId);

            // Apply date filters
            if (startDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.ReservationDate <= endDate.Value);
            }

            var reservations = await query
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }

        // PUT: api/users/reservation/{reservationId}/status - Update reservation status (owner only)
        [HttpPut("reservation/{reservationId}/status")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> UpdateReservationStatus(
            int reservationId,
            [FromBody] UpdateReservationStatusDto statusDto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            // Verify ownership
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (reservation.Restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Update status
            reservation.Status = statusDto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reservation {reservationId} status updated to {statusDto.Status}");

            return Ok(new { message = "Reservation status updated successfully" });
        }

        // POST: api/users/reservation/{reservationId}/checkin - Mark customer as seated
        [HttpPost("reservation/{reservationId}/checkin")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> CheckInReservation(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            // Verify ownership
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (reservation.Restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Validate current status
            if (reservation.Status != Enums.ReservationStatus.Confirmed)
                return BadRequest(new { message = "Reservation must be confirmed before check-in" });

            // Update to seated status
            reservation.Status = Enums.ReservationStatus.Seated;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Customer checked in successfully" });
        }
    }
}