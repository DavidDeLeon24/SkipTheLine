using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SkipTheLine.DTOs;
using SkipTheLine.Models;
using SkipTheLine.Data;
using SkipTheLine.Enums;
using SkipTheLine.Services;
using System.Security.Claims;

namespace SkipTheLine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(
            ApplicationDbContext context,
            IMapper mapper,
            UserManager<User> userManager,
            INotificationService notificationService,
            ILogger<ReservationsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var reservations = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetUpcomingReservations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var today = DateTime.Today;

            var reservations = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .Where(r => r.UserId == userId &&
                           r.ReservationDate >= today &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.Completed &&
                           r.Status != ReservationStatus.NoShow)
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservationHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var today = DateTime.Today;

            var reservations = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .Where(r => r.UserId == userId &&
                           (r.ReservationDate < today ||
                            r.Status == ReservationStatus.Completed ||
                            r.Status == ReservationStatus.Cancelled ||
                            r.Status == ReservationStatus.NoShow))
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReservationDto>>(reservations));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (reservation.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("RestaurantOwner"))
                return Forbid();

            return Ok(_mapper.Map<ReservationDto>(reservation));
        }

        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation(CreateReservationDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == createDto.RestaurantId);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (createDto.ReservationTime < restaurant.OpeningTime ||
                createDto.ReservationTime > restaurant.ClosingTime)
            {
                return BadRequest(new { message = $"Reservation time must be between {restaurant.OpeningTime:hh\\:mm} and {restaurant.ClosingTime:hh\\:mm}" });
            }

            if (createDto.ReservationDate.Date < DateTime.Today)
            {
                return BadRequest(new { message = "Cannot make reservations for past dates" });
            }

            if (createDto.PartySize > restaurant.MaxPartySize)
            {
                return BadRequest(new { message = $"Party size cannot exceed {restaurant.MaxPartySize}" });
            }

            var existingReservations = await _context.Reservations
                .Where(r => r.RestaurantId == createDto.RestaurantId &&
                           r.ReservationDate.Date == createDto.ReservationDate.Date &&
                           r.ReservationTime == createDto.ReservationTime &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.NoShow)
                .Select(r => r.TableId)
                .ToListAsync();

            var availableTable = restaurant.Tables
                .Where(t => t.Seats >= createDto.PartySize &&
                           t.IsActive &&
                           !existingReservations.Contains(t.Id))
                .OrderBy(t => t.Seats)
                .FirstOrDefault();

            if (availableTable == null)
            {
                return BadRequest(new { message = "No tables available for the selected time and party size" });
            }

            var reservation = new Reservation
            {
                UserId = userId,
                RestaurantId = createDto.RestaurantId,
                TableId = availableTable.Id,
                ReservationDate = createDto.ReservationDate,
                ReservationTime = createDto.ReservationTime,
                PartySize = createDto.PartySize,
                SpecialRequests = createDto.SpecialRequests,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var createdReservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            // Send notifications
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && createdReservation != null && createdReservation.Restaurant != null)
                {
                    await _notificationService.SendConfirmationEmailAsync(createdReservation, user, createdReservation.Restaurant);
                    await _notificationService.SendOwnerNotificationAsync(createdReservation, createdReservation.Restaurant);
                    _logger.LogInformation($"Reservation {reservation.Id} created and notifications sent");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notifications for reservation {reservation.Id}");
            }

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id },
                _mapper.Map<ReservationDto>(createdReservation));
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            // Check if user owns this reservation
            var isOwner = reservation.UserId == userId;
            var isAdmin = User.IsInRole("Admin");

            // Check if user is the restaurant owner
            bool isRestaurantOwner = false;
            if (!isOwner && !isAdmin)
            {
                var restaurant = await _context.Restaurants
                    .FirstOrDefaultAsync(r => r.Id == reservation.RestaurantId);
                isRestaurantOwner = restaurant != null && restaurant.OwnerId == userId;
            }

            if (!isOwner && !isAdmin && !isRestaurantOwner)
            {
                return Forbid(); // User cannot cancel this reservation
            }

            if (reservation.ReservationDate.Date < DateTime.Today)
                return BadRequest(new { message = "Cannot cancel past reservations" });

            if (reservation.Status == ReservationStatus.Cancelled)
                return BadRequest(new { message = "Reservation is already cancelled" });

            if (reservation.Status == ReservationStatus.Completed ||
                reservation.Status == ReservationStatus.Seated ||
                reservation.Status == ReservationStatus.NoShow)
            {
                return BadRequest(new { message = $"Cannot cancel reservation that is already {reservation.Status}" });
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reservation {id} cancelled by user {userId}");

            // Send cancellation notification only to the reservation owner
            if (isOwner)
            {
                try
                {
                    await _notificationService.SendCancellationEmailAsync(reservation, reservation.User, reservation.Restaurant);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send cancellation email for reservation {id}");
                }
            }

            return Ok(new { message = "Reservation cancelled successfully" });
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> RescheduleReservation(int id, [FromBody] RescheduleReservationDto rescheduleDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var reservation = await _context.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.Restaurant.Tables)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (reservation.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (reservation.Status == ReservationStatus.Cancelled)
                return BadRequest(new { message = "Cannot reschedule a cancelled reservation" });

            var restaurant = reservation.Restaurant;

            if (rescheduleDto.ReservationTime < restaurant.OpeningTime ||
                rescheduleDto.ReservationTime > restaurant.ClosingTime)
            {
                return BadRequest(new { message = $"Reservation time must be between {restaurant.OpeningTime:hh\\:mm} and {restaurant.ClosingTime:hh\\:mm}" });
            }

            var existingReservations = await _context.Reservations
                .Where(r => r.RestaurantId == restaurant.Id &&
                           r.Id != id &&
                           r.ReservationDate.Date == rescheduleDto.ReservationDate.Date &&
                           r.ReservationTime == rescheduleDto.ReservationTime &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.NoShow)
                .Select(r => r.TableId)
                .ToListAsync();

            var availableTable = restaurant.Tables
                .Where(t => t.Seats >= reservation.PartySize &&
                           t.IsActive &&
                           !existingReservations.Contains(t.Id))
                .OrderBy(t => t.Seats)
                .FirstOrDefault();

            if (availableTable == null)
            {
                return BadRequest(new { message = "No tables available for the selected time" });
            }

            reservation.ReservationDate = rescheduleDto.ReservationDate;
            reservation.ReservationTime = rescheduleDto.ReservationTime;
            reservation.TableId = availableTable.Id;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try
            {
                if (reservation.User != null && reservation.Restaurant != null)
                {
                    await _notificationService.SendConfirmationEmailAsync(reservation, reservation.User, reservation.Restaurant);
                    _logger.LogInformation($"Reservation {id} rescheduled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send reschedule notification for reservation {id}");
            }

            return Ok(new { message = "Reservation rescheduled successfully" });
        }

        [HttpGet("restaurant/{restaurantId}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvailabilitySlotDto>>> GetAvailability(
            int restaurantId,
            [FromQuery] DateTime date,
            [FromQuery] int partySize)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (partySize > restaurant.MaxPartySize)
                return BadRequest(new { message = $"Party size exceeds maximum of {restaurant.MaxPartySize}" });

            if (date.Date < DateTime.Today)
                return BadRequest(new { message = "Cannot check availability for past dates" });

            var reservations = await _context.Reservations
                .Where(r => r.RestaurantId == restaurantId &&
                           r.ReservationDate.Date == date.Date &&
                           r.Status != ReservationStatus.Cancelled &&
                           r.Status != ReservationStatus.NoShow)
                .ToListAsync();

            var availabilitySlots = new List<AvailabilitySlotDto>();
            var startTime = restaurant.OpeningTime;
            var endTime = restaurant.ClosingTime;
            var interval = TimeSpan.FromMinutes(30);

            for (var time = startTime; time < endTime; time = time.Add(interval))
            {
                var availableTables = restaurant.Tables
                    .Where(t => t.Seats >= partySize && t.IsActive)
                    .ToList();

                var bookedTableIds = reservations
                    .Where(r => r.ReservationTime == time)
                    .Select(r => r.TableId)
                    .ToList();

                var availableTableSizes = availableTables
                    .Where(t => !bookedTableIds.Contains(t.Id))
                    .Select(t => t.Seats)
                    .OrderBy(s => s)
                    .ToList();

                if (availableTableSizes.Any())
                {
                    availabilitySlots.Add(new AvailabilitySlotDto
                    {
                        Time = time.ToString(@"hh\:mm"),
                        AvailableTables = availableTableSizes.Count,
                        TableSizes = availableTableSizes
                    });
                }
            }

            return Ok(availabilitySlots);
        }

        [HttpGet("restaurant/{restaurantId}/stats")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<ActionResult<object>> GetRestaurantStats(int restaurantId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var allReservations = await _context.Reservations
                .Where(r => r.RestaurantId == restaurantId)
                .ToListAsync();

            var stats = new
            {
                total = new
                {
                    reservations = allReservations.Count,
                    confirmed = allReservations.Count(r => r.Status == ReservationStatus.Confirmed),
                    completed = allReservations.Count(r => r.Status == ReservationStatus.Completed),
                    cancelled = allReservations.Count(r => r.Status == ReservationStatus.Cancelled),
                    noShow = allReservations.Count(r => r.Status == ReservationStatus.NoShow)
                },
                today = new
                {
                    reservations = allReservations.Count(r => r.ReservationDate.Date == today),
                    upcoming = allReservations.Count(r => r.ReservationDate.Date == today &&
                                                          r.ReservationTime > DateTime.Now.TimeOfDay &&
                                                          r.Status == ReservationStatus.Confirmed),
                    seated = allReservations.Count(r => r.ReservationDate.Date == today &&
                                                        r.Status == ReservationStatus.Seated)
                },
                weekly = new
                {
                    reservations = allReservations.Count(r => r.ReservationDate >= startOfWeek),
                    averagePartySize = allReservations
                        .Where(r => r.ReservationDate >= startOfWeek)
                        .DefaultIfEmpty()
                        .Average(r => r == null ? 0 : r.PartySize)
                },
                monthly = new
                {
                    reservations = allReservations.Count(r => r.ReservationDate >= startOfMonth),
                    averagePartySize = allReservations
                        .Where(r => r.ReservationDate >= startOfMonth)
                        .DefaultIfEmpty()
                        .Average(r => r == null ? 0 : r.PartySize)
                },
                popularTimes = allReservations
                    .GroupBy(r => r.ReservationTime.Hours)
                    .Select(g => new { hour = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .Take(5)
            };

            return Ok(stats);
        }
    }

    public class RescheduleReservationDto
    {
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
    }
}