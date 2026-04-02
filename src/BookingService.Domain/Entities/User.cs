using System.ComponentModel.DataAnnotations;

namespace BookingService.Domain.Entities
{
    public class User
    {
        // Primary Key
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        public string LastName { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        // Optional: List of bookings made by this guest
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        
        public UserStatus UserStatus { get; set; }
    }
    
    public enum UserStatus
    {
        Active,
        Unactive
    }
}