using System;

namespace BookingService.Application.Dto
{
    public class BookingDto
    {
        public Guid Id { get; set; } // Unique identifier for the booking
        public int DayIndex  { get; set; }
        public int StartHourIndex { get; set; }

        public double Duration { get; set; }
        public string Title { get; set; }
        public string Room { get; set; }
        public ColorTheme Color { get; set; }

        //public Guid RoomId { get; set; }
    //    public DateTime StartDate { get; set; }
  //      public DateTime EndDate { get; set; }
   //     public string GuestName { get; set; }
    //    public string GuestEmail { get; set; }
  //      public string Status { get; set; } // e.g., "Pending", "Confirmed", "Cancelled", "Completed"
   //     public DateTime CreatedAt { get; set; }
   //     public DateTime? UpdatedAt { get; set; }
    }
    public enum ColorTheme
    {
        Teal,
        Purple
    }
}