using System;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Dto
{
    public class CreateBookingDto
    {
        [Required]
        public Guid MeetingRoomId { get; set; } 
        
        [Required]
        public Guid UserId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Guest name cannot be longer than 100 characters.")]
        public string UserName { get; set; } // Name of the person making the booking
        
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
        
        [EmailAddress]
        [StringLength(255)]
        public string UserEmail { get; set; } // Email of the guest
    }
}