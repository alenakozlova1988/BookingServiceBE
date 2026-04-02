using System;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Dto
{
    public class UpdateBookingDto
    {
        // Note: We might not allow changing RoomId after creation, or it might be a complex operation.
        // Let's assume for this example we can update dates and guest info.

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Guest name cannot be longer than 100 characters.")]
        public string GuestName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string GuestEmail { get; set; }

        // Optional: Consider how to handle status updates. Maybe a separate endpoint or specific DTO for cancellation.
        // public string Status { get; set; }
    }
}