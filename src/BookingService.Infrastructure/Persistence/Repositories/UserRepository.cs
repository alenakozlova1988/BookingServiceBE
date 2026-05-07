using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<User>> GetByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> AddAsync(User entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // For added safety, generate ID if not already set (though it's default)
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        await _context.Users.AddAsync(entity);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var kkk = ex.Message;
        } // Save changes to persist the booking to the DB

        return entity.Id; // Return the booking with its ID assigned
    }

    public void Update(User entity)
    {
        throw new NotImplementedException();
    }

    public void Delete(User booking)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}