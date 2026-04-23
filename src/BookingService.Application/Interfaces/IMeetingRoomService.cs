using BookingService.Application.Dto;

namespace BookingService.Application.Services
{
    public interface IMeetingRoomService
    {
        Task<BookingDto> CreateMeetingRoomAsync(CreateBookingDto bookingDto);
        
        Task<IEnumerable<MeetingRoomDto>> GetAllMeetingRoomsAsync();
        Task<BookingDto?> GetMeetingRoomByIdAsync(Guid id);
        Task<IEnumerable<BookingDto>> GetBookingsByUserIdAsync(Guid userId);
        Task<bool> CancelBookingAsync(Guid bookingId);
    }
}