using System.ComponentModel.DataAnnotations;

namespace BookingService.Domain.Entities;

public class MeetingRoom
{
    // Primary Key
    public Guid Id { get; set; } = Guid.NewGuid(); // Default to new GUID

    // Foreign Key to RoomType
    public Guid MeetingRoomTypeId { get; set; }

    [StringLength(255)]
    public string Description { get; set; } // Optional description for this specific room

    // Max Occupancy for this room
    public int MaxOccupancy { get; set; }

    // Status of the room (e.g., "Available", "Occupied", "Maintenance")
    // Consider using an Enum for this (see below)
    public Status Status { get; set; } = Status.Available;
    
    public RoomColor Color { get; set; }

    // Navigation Property to Bookings
    public ICollection<Booking> Bookings { get; set;} = new List<Booking>();
}

// Example Enum for Room Status
public enum Status
{
    Available,
    Occupied,
    Maintenance,
    OutOfService
}

public enum RoomColor
{
    Teal,
    Blue,
    Purple
}