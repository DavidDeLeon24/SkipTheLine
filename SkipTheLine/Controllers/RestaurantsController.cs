using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SkipTheLine.DTOs;
using SkipTheLine.Models;
using SkipTheLine.Data;
using SkipTheLine.Enums;
using System.Security.Claims;

namespace SkipTheLine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantsController> _logger;

        public RestaurantsController(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<RestaurantsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/restaurants
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurants(
            [FromQuery] string? searchTerm,
            [FromQuery] string? cuisine,
            [FromQuery] string? city)
        {
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm) ||
                                         r.Cuisine.Contains(searchTerm) ||
                                         r.City.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(cuisine))
            {
                query = query.Where(r => r.Cuisine == cuisine);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(r => r.City.Contains(city));
            }

            var restaurants = await query
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(_mapper.Map<List<RestaurantDto>>(restaurants));
        }

        // GET: api/restaurants/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            return Ok(_mapper.Map<RestaurantDto>(restaurant));
        }

        // GET: api/restaurants/cuisines
        [HttpGet("cuisines")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetCuisines()
        {
            var cuisines = await _context.Restaurants
                .Select(r => r.Cuisine)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(cuisines);
        }

        // GET: api/restaurants/cities
        [HttpGet("cities")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<string>>> GetCities()
        {
            var cities = await _context.Restaurants
                .Select(r => r.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(cities);
        }

        // GET: api/restaurants/my-restaurants
        [HttpGet("my-restaurants")]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetMyRestaurants()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == userId)
                .Include(r => r.Tables)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(_mapper.Map<List<RestaurantDto>>(restaurants));
        }

        // POST: api/restaurants
        [HttpPost]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<ActionResult<RestaurantDto>> CreateRestaurant(CreateRestaurantDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Check if restaurant with same name and address exists
            var existingRestaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Name == createDto.Name && r.Address == createDto.Address);

            if (existingRestaurant != null)
                return BadRequest(new { message = "A restaurant with this name and address already exists" });

            var restaurant = _mapper.Map<Restaurant>(createDto);
            restaurant.OwnerId = userId;
            restaurant.CreatedAt = DateTime.UtcNow;
            restaurant.Rating = 0;
            restaurant.TotalReviews = 0;

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Create default tables for the restaurant
            var tables = new List<Table>();

            // 2-seater tables (6 tables)
            for (int i = 1; i <= 6; i++)
            {
                tables.Add(new Table
                {
                    RestaurantId = restaurant.Id,
                    TableNumber = i,
                    Seats = 2,
                    IsActive = true
                });
            }

            // 4-seater tables (4 tables)
            for (int i = 7; i <= 10; i++)
            {
                tables.Add(new Table
                {
                    RestaurantId = restaurant.Id,
                    TableNumber = i,
                    Seats = 4,
                    IsActive = true
                });
            }

            // 6-seater tables (2 tables)
            for (int i = 11; i <= 12; i++)
            {
                tables.Add(new Table
                {
                    RestaurantId = restaurant.Id,
                    TableNumber = i,
                    Seats = 6,
                    IsActive = true
                });
            }

            _context.Tables.AddRange(tables);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Restaurant {restaurant.Name} created by user {userId} with {tables.Count} tables");

            return Ok(_mapper.Map<RestaurantDto>(restaurant));
        }

        // PUT: api/restaurants/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> UpdateRestaurant(int id, UpdateRestaurantDto updateDto)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _mapper.Map(updateDto, restaurant);
            restaurant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Restaurant {restaurant.Name} updated by user {userId}");

            return Ok(_mapper.Map<RestaurantDto>(restaurant));
        }

        // DELETE: api/restaurants/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Reservations)
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            // Check if there are future reservations
            var hasFutureReservations = restaurant.Reservations
                .Any(r => r.ReservationDate >= DateTime.Today &&
                          r.Status != ReservationStatus.Cancelled);

            if (hasFutureReservations)
                return BadRequest(new { message = "Cannot delete restaurant with future reservations" });

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Restaurant {restaurant.Name} deleted by admin");

            return Ok(new { message = "Restaurant deleted successfully" });
        }

        // GET: api/restaurants/{id}/availability
        [HttpGet("{id}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvailabilitySlotDto>>> GetAvailability(
            int id,
            [FromQuery] DateTime date,
            [FromQuery] int partySize)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (partySize > restaurant.MaxPartySize)
                return BadRequest(new { message = $"Party size exceeds maximum of {restaurant.MaxPartySize}" });

            if (date.Date < DateTime.Today)
                return BadRequest(new { message = "Cannot check availability for past dates" });

            var reservations = await _context.Reservations
                .Where(r => r.RestaurantId == id &&
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

        // GET: api/restaurants/{id}/tables
        [HttpGet("{id}/tables")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetRestaurantTables(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var tables = await _context.Tables
                .Where(t => t.RestaurantId == id)
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            return Ok(tables.Select(t => new
            {
                t.Id,
                t.TableNumber,
                t.Seats,
                t.IsActive
            }));
        }

        // PUT: api/restaurants/{id}/tables/{tableId}/toggle
        [HttpPut("{id}/tables/{tableId}/toggle")]
        [Authorize(Roles = "RestaurantOwner,Admin")]
        public async Task<IActionResult> ToggleTableStatus(int id, int tableId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null)
                return NotFound(new { message = "Restaurant not found" });

            if (restaurant.OwnerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == tableId && t.RestaurantId == id);

            if (table == null)
                return NotFound(new { message = "Table not found" });

            table.IsActive = !table.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Table #{table.TableNumber} is now {(table.IsActive ? "active" : "inactive")}" });
        }
    }
}