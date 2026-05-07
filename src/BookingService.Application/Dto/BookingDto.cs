using System;

namespace BookingService.Application.Dto
{
    public class BookingDto
    {
        public Guid Id { get; set; } // Unique identifier for the booking
        public int DayIndex  { get; set; }
        public int StartHourIndex { get; set; }
        public DateTime Date { get; set; }
        public double Duration { get; set; }
        public string Title { get; set; }
        public string RoomDescription { get; set; }
        public string RoomId { get; set; }
        public ColorTheme Color { get; set; }
        public string AuthorName { get; set; }
    }
    public enum ColorTheme
    {
        Teal,
        Purple,
    }
}