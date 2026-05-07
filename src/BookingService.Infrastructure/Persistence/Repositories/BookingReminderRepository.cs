using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingReminderRepository : IBookingReminderRepository
{
    private readonly AppDbContext _context;

    public BookingReminderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetTodayBookingsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _context.Bookings
            .Include(x => x.User)
            .Where(b => b.CheckInDate >= today && b.CheckInDate < tomorrow && b.Status != BookingStatus.Cancelled)
            .ToListAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}