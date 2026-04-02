namespace BookingService.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public MeetingRoom? MeetingRoom { get; set; }
    public Guid MeetingRoomId { get; set; }
    
    public User? User { get; set; }
    public Guid UserId { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed,
    NoShow
}