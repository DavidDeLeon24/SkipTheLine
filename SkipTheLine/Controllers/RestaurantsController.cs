using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkipTheLine.Data;
using SkipTheLine.DTOs;
using SkipTheLine.Enums;
using SkipTheLine.Models;
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

        public RestaurantsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurants()
        {
            var restaurants = await _context.Restaurants
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(_mapper.Map<List<RestaurantDto>>(restaurants));
        }

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
    }
}