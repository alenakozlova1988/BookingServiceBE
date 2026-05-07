using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;
    
    public RoomRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<MeetingRoom?> GetByIdAsync(Guid id)
    {
        return await _context.MeetingRooms.FirstOrDefaultAsync(x =>x.Id == id);
    }

    public async Task<IEnumerable<MeetingRoom>> GetAllAsync()
    {
        return await _context.MeetingRooms.ToListAsync();
    }

    public Task<IEnumerable<MeetingRoom>> GetByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> AddAsync(MeetingRoom entity)
    {
        throw new NotImplementedException();
    }

    public void Update(MeetingRoom entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(MeetingRoom booking)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}