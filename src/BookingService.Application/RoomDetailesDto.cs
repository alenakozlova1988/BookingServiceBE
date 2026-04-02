namespace BookingService.Application;

public class RoomDetailsDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; }
    public string RoomType { get; set; }
    public decimal BasePricePerNight { get; set; }
    public int Capacity { get; set; }
}