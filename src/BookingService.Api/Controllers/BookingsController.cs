using BookingService.Application.Dto;
using BookingService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // If authentication is used

namespace BookingService.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    // [Authorize] // Uncomment if authentication is implemented
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Assume UserId is extracted from claims if [Authorize] is used
                // bookingDto.UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var createdBooking = await _bookingService.CreateBookingAsync(bookingDto);
                // Use CreatedAtAction for RESTful best practices
                return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogWarning(knfEx, "Resource not found during booking creation.");
                return NotFound(knfEx.Message);
            }
            catch (InvalidOperationException ioEx) // For payment failures, etc.
            {
                 _logger.LogError(ioEx, "Business logic error during booking creation.");
                return BadRequest(new { error = ioEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during booking creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingDto>> GetBookingById(Guid id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found.");
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving booking {BookingId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BookingDto>> GetAll()
        {
            try
            {
                var booking = await _bookingService.GetAllBookingsAsync();
                if (!booking.Any())
                {
                    return NotFound($"No any bookings found.");
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving booking {BookingId}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }

        // Add GetBookingsByUserId, CancelBooking, etc. endpoints similarly

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
         [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
             try
            {
                var success = await _bookingService.CancelBookingAsync(id);
                if (!success)
                {
                     // Could differentiate between not found and already cancelled/completed
                     // For simplicity, return BadRequest for any non-success
                    return BadRequest($"Could not cancel booking with ID {id}. It might be already cancelled, completed, or does not exist.");
                }
                return NoContent(); // Standard response for successful deletion/cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cancelling booking {BookingId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred.");
            }
        }
    }
}
