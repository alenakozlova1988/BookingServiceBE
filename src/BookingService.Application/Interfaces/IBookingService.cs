using BookingService.Application.Dto;

namespace BookingService.Application.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto bookingDto);
        
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string? roomId = null);
        Task<BookingDto?> GetBookingByIdAsync(Guid id);
        Task<IEnumerable<BookingDto>> GetBookingsByUserIdAsync(Guid userId);
        Task<bool> CancelBookingAsync(Guid bookingId);
        Task<BookingDto> UpdateBookingAsync(Guid bookingId, CreateBookingDto bookingDto);
    }
}