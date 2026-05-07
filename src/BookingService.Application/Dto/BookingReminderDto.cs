namespace BookingService.Application.Dto;

public class BookingReminderDto
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
}