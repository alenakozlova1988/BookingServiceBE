using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingService.Domain;
using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Persistence; // Import your DbContext
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence.Repositories
{
    // Concrete implementation of the IBookingRepository using Entity Framework Core
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private IQueryable<Booking> IncludeRelationships(IQueryable<Booking> query)
        {
            return query
                .Include(b => b.MeetingRoom) // Include the Room enti
                .Include(b => b.User); // Include the Guest entity
        }

        public async Task<Booking?> GetByIdAsync(Guid id, bool includeRelationships = false)
        {
            if (id == Guid.Empty) return null;

            IQueryable<Booking> query = _context.Bookings.AsQueryable();
            if (includeRelationships)
            {
                query = IncludeRelationships(query);
            }

            // Use singleOrDefaultAsync to find the booking or return null if not found
            return await query.FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<Booking?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(x => x.User)
                .Include(x => x.MeetingRoom)
                .Where(x => x.Status != BookingStatus.NoShow).ToListAsync();
        }

        public Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void Update(Booking booking)
        {
            throw new NotImplementedException();
        }

        public void Delete(Booking booking)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Guid> AddAsync(Booking booking)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));

            // For added safety, generate ID if not already set (though it's default)
            if (booking.Id == Guid.Empty)
            {
                booking.Id = Guid.NewGuid();
            }

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync(); // Save changes to persist the booking to the DB
            return booking.Id; // Return the booking with its ID assigned
        }

        public async Task UpdateAsync(Booking booking)
        {
            if (booking == null) throw new ArgumentNullException(nameof(booking));
            if (booking.Id == Guid.Empty) throw new ArgumentException("Booking must have a valid ID for update.", nameof(booking));

            // EF Core tracks changes. If the entity is already tracked, it will be updated.
            // If it's detached, we might need to attach it or use this logic:
            var existingBooking = await _context.Bookings.FindAsync(booking.Id);
            if (existingBooking == null)
            {
                throw new KeyNotFoundException($"Booking with ID {booking.Id} not found.");
                // Or, alternatively, you could add it if you expect additions via update:
                // _context.Bookings.Add(booking);
            }
            else
            {
                _context.Entry(existingBooking).CurrentValues.SetValues(booking);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty) return; // Or throw an exception if ID is required

            var bookingToDelete = await _context.Bookings.FindAsync(id);
            if (bookingToDelete != null)
            {
                _context.Bookings.Remove(bookingToDelete);
                await _context.SaveChangesAsync();
            }
            // Optionally, throw an exception if not found:
            else
            {
                throw new KeyNotFoundException($"Booking with ID {id} not found.");
            }
        }

        public async Task<IEnumerable<Booking>> GetAllAsync(bool includeRelationships = false)
        {
            IQueryable<Booking> query = _context.Bookings.AsQueryable();
            if (includeRelationships)
            {
                query = IncludeRelationships(query);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Booking>> FindBookingsAsync(
            Guid? guestId = null,
            Guid? roomId = null,
            DateTime? checkInDate = null,
            DateTime? checkOutDate = null,
            bool includeRelationships = false)
        {
            IQueryable<Booking> query = _context.Bookings.AsQueryable();

            if (includeRelationships)
            {
                query = IncludeRelationships(query);
            }

            if (guestId.HasValue && guestId != Guid.Empty)
            {
                query = query.Where(b => b.UserId == guestId.Value);
            }

            if (roomId.HasValue && roomId != Guid.Empty)
            {
                query = query.Where(b => b.MeetingRoomId == roomId.Value);
            }

            // Example: Find bookings that overlap with the given date range
            // A booking overlaps if:
            // 1. Its check-in is before our check-out AND its check-out is after our check-in.
            if (checkInDate.HasValue)
            {
                query = query.Where(b => b.CheckOutDate > checkInDate.Value); // Booking ends after our check-in
            }
            if (checkOutDate.HasValue)
            {
                query = query.Where(b => b.CheckInDate < checkOutDate.Value); // Booking starts before our check-out
            }
            // Ensure that dates are compared appropriately. If your dates include time, you might need to adjust.

            return await query.ToListAsync();
        }


        public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkIn, DateTime checkOut)
        {
            if (roomId == Guid.Empty) throw new ArgumentException("Room ID cannot be empty.", nameof(roomId));
            if (checkIn >= checkOut) throw new ArgumentException("Check-in date must be before check-out date.");

            // Find bookings for the specified room that *overlap* with the requested date range.
            // Overlap occurs if:
            // - The existing booking starts before the requested check-out date, AND
            // - The existing booking ends after the requested check-in date.
            var overlappingBooking = await _context.Bookings
                .AnyAsync(b => b.MeetingRoomId == roomId &&
                               b.CheckOutDate > checkIn && // Existing booking ends after requested check-in
                               b.CheckInDate < checkOut   // Existing booking starts before requested check-out
                );

            return !overlappingBooking; // Room is available if no overlapping booking is found.
        }

        // --- Add other implementations for custom methods ---
    }
}
