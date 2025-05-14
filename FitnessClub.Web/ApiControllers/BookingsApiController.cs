using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessClub.BLL.Enums;
using System.Security.Claims;

namespace FitnessClub.Web.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsApiController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;

        public BookingsApiController(IBookingService bookingService, IUserService userService)
        {
            _bookingService = bookingService;
            _userService = userService;
        }

        // GET: api/BookingsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
        {
            var bookings = await _bookingService.GetAllBookingsAsync(); 
            return Ok(bookings);
        }
        
        // GET: api/Users/{userId}/Bookings 
        [HttpGet("/api/Users/{userId}/Bookings")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetUserBookings(int userId)
        {
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // GET: api/BookingsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound($"Бронювання з ID {id} не знайдено.");
            return Ok(booking);
        }

        // POST: api/BookingsApi
        [HttpPost]
        public async Task<ActionResult<BookingDto>> PostBooking([FromBody] CreateBookingRequestDto createBookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, bookingIdStr) = await _bookingService.BookClassAsync(
                createBookingDto.UserId, 
                createBookingDto.GuestName, 
                createBookingDto.ClassScheduleId, 
                createBookingDto.ClassDate);

            if (result == BookingResult.Success && int.TryParse(bookingIdStr, out int newBookingId))
            {
                var createdBooking = await _bookingService.GetBookingByIdAsync(newBookingId);
                if (createdBooking != null) 
                {
                    return CreatedAtAction(nameof(GetBooking), new { id = newBookingId }, createdBooking);
                }
                return StatusCode(201, new { BookingId = newBookingId, Message = "Бронювання успішне." });
            }
            else
            {
                string failureReason = result.ToString();
                if (!string.IsNullOrEmpty(bookingIdStr))
                {
                    failureReason = bookingIdStr;
                }
                return BadRequest($"Не вдалося забронювати: {failureReason}");
            }
        }

        // PUT: api/BookingsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(int id, [FromBody] BookingDto bookingDto)
        {
            if (id != bookingDto.BookingId)
            {
                return BadRequest("ID в URL має співпадати з ID в тілі запиту.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var success = await _bookingService.UpdateBookingAsync(id, bookingDto); 
            if (!success) 
            {
                return NotFound($"Бронювання з ID {id} не знайдено або оновлення не вдалося.");
            }
            return NoContent();
        }

        // DELETE: api/BookingsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int userIdForCancellation))
            {
                return Unauthorized("Не вдалося визначити ID користувача для скасування бронювання.");
            }

            var success = await _bookingService.CancelBookingAsync(id, userIdForCancellation);
            if (!success)
            {
                return NotFound($"Бронювання з ID {id} не знайдено, або скасування не вдалося (не авторизовано, вже скасовано, або не належить користувачу {userIdForCancellation}).");
            }
            return NoContent();
        }
    }
} 