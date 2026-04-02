using BookingService.Application.Dto;
using BookingService.Application.Mapping; // For AutoMapper
using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Integrations.RoomMgmt; // Example
//using BookingService.Infrastructure.Messaging; // Example
using AutoMapper;
using BookingService.Application.Dto;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomManagementService _roomManagementService;
   //     private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IRoomManagementService roomManagementService,
         //   IMessagePublisher messagePublisher,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _roomManagementService = roomManagementService;
          //  _messagePublisher = messagePublisher;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            // 1. Validate input (handled by API layer mostly, but can add business logic validation here)
            // 2. Fetch Room Details (Check availability if not handled by Room Service)
            var roomDetails = await _roomManagementService.GetRoomDetailsAsync(bookingDto.MeetingRoomId);
            if (roomDetails == null) // Or specific exception for not found
            {
                throw new KeyNotFoundException($"Room with ID {bookingDto.MeetingRoomId} not found.");
            }
            
            // 4. Create Booking Entity
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                MeetingRoomId = bookingDto.MeetingRoomId,
                UserId = bookingDto.UserId, // Assume UserId comes from auth token or DTO
                CheckInDate = bookingDto.CheckInDate,
                CheckOutDate = bookingDto.CheckOutDate,
                Status = BookingStatus.Pending // Initial status
            };

            // 5. Save Booking to DB
            await _bookingRepository.AddAsync(booking);
            try
            {
                await _bookingRepository.SaveChangesAsync(); // Save changes before payment/messaging
            }
            catch (Exception ex)
            {
                var kk = ex.Message;
            }

            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return (bookings.Any() ? _mapper.Map<IEnumerable<BookingDto>>(bookings) : null) 
                   ?? throw new InvalidOperationException();
        }

        public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking != null ? _mapper.Map<BookingDto>(booking) : null;
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByUserIdAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Attempted to cancel non-existent booking {BookingId}.", bookingId);
                return false;
            }

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                _logger.LogWarning("Attempted to cancel booking {BookingId} with status {Status}.", bookingId, booking.Status);
                return false; // Already cancelled or completed
            }

            // Potentially initiate refund via Payment Service
            // ...

            booking.Status = BookingStatus.Cancelled;
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();

          //  await _messagePublisher.PublishAsync("booking.cancelled", new BookingCancelledEventDto { BookingId = bookingId, UserId = booking.UserId });

            _logger.LogInformation("Booking {BookingId} cancelled successfully.", bookingId);
            return true;
        }

        public Task<BookingDto> UpdateBookingAsync(Guid bookingId, UpdateBookingDto bookingDto)
        {
            // Implement logic for updating booking (e.g., changing dates, requires re-validation and price recalculation)
            throw new NotImplementedException();
        }
    }
}
