using BookingService.Application.Dto;
using BookingService.Application.Mapping; // For AutoMapper
using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Integrations.RoomMgmt; // Example
//using BookingService.Infrastructure.Messaging; // Example
using AutoMapper;
using BookingService.Application.Dto;
using BookingService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        private readonly IRoomManagementService _roomManagementService;
        private readonly BookingMetricsService _metrics;

        //     private readonly IMessagePublisher _messagePublisher;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;
        private readonly ICurrentUserService _currentUser;

        public BookingService(
            IBookingRepository bookingRepository,
            IRoomManagementService roomManagementService,
            ICurrentUserService currentUser,
            IMapper mapper,
            ILogger<BookingService> logger,
            BookingMetricsService metrics)
        {
            _bookingRepository = bookingRepository;
            _roomManagementService = roomManagementService;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
            _metrics = metrics;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            using (LogContext.PushProperty("RoomId", bookingDto.RoomId))
            {
                using var duration = _metrics.MeasureBookingDuration(bookingDto.RoomId);
                try
                {
                    if (_currentUser?.UserId == null)
                    {
                        _logger.LogError("Пользователь не авторизован  в системе.");
                        throw new Exception("User not found");
                    }

                    var room = await _roomManagementService.GetRoomDetailsAsync(Guid.Parse(bookingDto.RoomId));
                  
                    if (room == null)
                    {
                        _logger.LogError("Переговорная комната не существует в системе. {@Room}", bookingDto.RoomId);
                        throw new Exception("Room not found");
                    }

                    var startDate =
                        new DateTime(bookingDto.Date.Year, bookingDto.Date.Month, bookingDto.Date.Day).AddHours(
                            bookingDto.StartHourIndex + 8);
                    var endDate = startDate.AddHours(bookingDto.Duration);

                    ValidateBookingPeriods(endDate, startDate);
                    // 4. Create Booking Entity
                    var booking = new Booking
                    {
                        Id = Guid.NewGuid(),
                        CheckInDate = startDate.ToUniversalTime(),//.UtcDateTime, //bookingDto.CheckInDate,
                        CheckOutDate = endDate.ToUniversalTime(),
                        Status = BookingStatus.Pending,
                        MeetingRoomId = Guid.Parse(bookingDto.RoomId),
                        Title = bookingDto.Title,
                        UserId = Guid.Parse(_currentUser?.UserId)
                    };

                    await _bookingRepository.AddAsync(booking); // Save changes before payment/messaging

                    _logger.LogInformation(
                        "Бронирование создано. {@Booking}",
                        new
                        {
                            EventType = "booking_created",
                            booking.Id,
                            booking.MeetingRoomId,
                            booking.UserId,
                            booking.CreatedAt
                        });
                    
                    _metrics.RecordBookingCreated(booking.MeetingRoomId.ToString(), _currentUser.UserId);

                    return _mapper.Map<BookingDto>(booking);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Ошибка создания бронирования. RoomId: {RoomId}, UserId: {UserId}",
                        bookingDto.RoomId, _currentUser.UserId);
                    _metrics.RecordBookingFailed();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string? roomId = null)
        {
                var bookings = await _bookingRepository.GetAllAsync(roomId);
                return _mapper.Map<IEnumerable<BookingDto>>(bookings);
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
            
            // Already cancelled or completed
            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
            {
                _logger.LogWarning("Attempted to cancel booking {BookingId} with status {Status}.", bookingId,
                    booking.Status);
                return false; 
            }
            
            _bookingRepository.Delete(booking);

            _logger.LogInformation("Booking {BookingId} cancelled successfully.", bookingId);
            return true;
        }

        public async Task<BookingDto> UpdateBookingAsync(Guid bookingId, CreateBookingDto bookingDto)
        {
            _logger.LogInformation("Попытка обновления бронирования с ID: {BookingId}", bookingId);

            // 1. Найти существующее бронирование
            var bookingToUpdate = await _bookingRepository.GetByIdAsync(bookingId);

            if (bookingToUpdate == null)
            {
                _logger.LogWarning("Бронирование с ID {BookingId} не найдено.", bookingId);
                throw new KeyNotFoundException($"Бронирование с ID '{bookingId}' не найдено.");
            }
            
            var startDate =
                new DateTime(bookingDto.Date.Year, bookingDto.Date.Month, bookingDto.Date.Day).AddHours(
                    bookingDto.StartHourIndex + 8);
            var endDate = startDate.AddHours(bookingDto.Duration);
            var roomId = Guid.Parse(bookingDto.RoomId);
            
             ValidateBookingPeriods(endDate, startDate);

             //Есть ли  пересечение с другими бронированиями (если время изменилось)
             await ValidateOverlappingPeriodsForRoom(bookingToUpdate, startDate, endDate);

             // 4. Обновить поля бронирования
             if(bookingToUpdate.CheckInDate != startDate.ToUniversalTime())
                bookingToUpdate.CheckInDate = startDate.ToUniversalTime();
             
             if(bookingToUpdate.CheckOutDate != endDate.ToUniversalTime())
                 bookingToUpdate.CheckOutDate = endDate.ToUniversalTime();
             
             if(bookingToUpdate.MeetingRoomId != roomId)
                 bookingToUpdate.MeetingRoomId = roomId;
             
             if(bookingToUpdate.Title != bookingDto.Title)
                 bookingToUpdate.Title = bookingDto.Title;

             try
             {
                 _bookingRepository.Update(bookingToUpdate);
                 _logger.LogInformation("Бронирование с ID {BookingId} успешно обновлено.", bookingId);
             }
             catch (DbUpdateException ex)
             {
                 _logger.LogError(ex, "Ошибка базы данных при обновлении бронирования {BookingId}.", bookingId);
                 throw; // Перебрасываем исключение, чтобы вызывающий код мог его обработать
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Неизвестная ошибка при обновлении бронирования {BookingId}.", bookingId);
                 throw;
             }
            
             return _mapper.Map<BookingDto>(bookingToUpdate);
        }

        private async Task ValidateOverlappingPeriodsForRoom(Booking bookingToUpdate, DateTime startDate,
            DateTime endDate)
        {
            if (bookingToUpdate.CheckInDate != startDate.ToUniversalTime() || bookingToUpdate.CheckOutDate != endDate.ToUniversalTime())
            {
                var overlappingBookings = await _bookingRepository.IsBookingOverlapsForRoom(bookingToUpdate, startDate, endDate);

                if (overlappingBookings)
                {
                    _logger.LogWarning("Попытка обновления бронирования {BookingId} привела к конфликту времени с другим бронированием.", bookingToUpdate.Id);
                    throw new InvalidOperationException("Выбранное время бронирования пересекается с другим существующим бронированием этого номера.");
                }
            }
        }

        private void ValidateBookingPeriods(DateTime endDate, DateTime startDate)
        {
            if (startDate >= endDate)
            {
                _logger.LogError(
                    "Ошибка валидации: Дата начала ({StartDate}) должна быть раньше даты окончания ({EndDate}).",
                    startDate, endDate);
                throw new ArgumentException("Дата начала должна быть раньше даты окончания.");
            }
        }
    }
}
