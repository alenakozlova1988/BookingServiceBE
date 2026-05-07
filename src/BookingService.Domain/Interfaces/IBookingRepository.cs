using BookingService.Domain.Entities;

namespace BookingService.Domain.Interfaces;

public interface IBookingRepository : IBaseRepository<Booking>
{
    Task<bool> IsBookingOverlapsForRoom(Booking booking, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Booking>> GetAllAsync(string? roomId = null);
}