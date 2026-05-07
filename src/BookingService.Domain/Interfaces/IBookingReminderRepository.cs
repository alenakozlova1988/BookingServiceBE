using BookingService.Domain.Entities;

namespace BookingService.Domain.Interfaces;

public interface IBookingReminderRepository
{
    Task<List<Booking>> GetTodayBookingsAsync();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}