using AutoMapper;
using BookingService.Application.Dto;
using BookingService.Domain.Interfaces;

namespace BookingService.Application.Services;

public class MeetingRoomService(IRoomRepository roomRepository, IMapper mapper) : IMeetingRoomService
{
    public Task<BookingDto> CreateMeetingRoomAsync(CreateBookingDto bookingDto)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<MeetingRoomDto>> GetAllMeetingRoomsAsync()
    {
        var rooms = await roomRepository.GetAllAsync();
        return (rooms.Any() ? mapper.Map<IEnumerable<MeetingRoomDto>>(rooms) : null)
               ?? throw new InvalidOperationException();
    }

    public Task<BookingDto?> GetMeetingRoomByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BookingDto>> GetBookingsByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CancelBookingAsync(Guid bookingId)
    {
        throw new NotImplementedException();
    }
}