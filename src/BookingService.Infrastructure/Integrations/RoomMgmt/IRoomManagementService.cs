// src/BookingService.Infrastructure/Integrations/RoomMgmt/IRoomManagementService.cs
using Microsoft.Extensions.Logging;
using BookingService.Domain.Interfaces;

namespace BookingService.Infrastructure.Integrations.RoomMgmt;
    public interface IRoomManagementService
    {
        Task<RoomDetailsDto?> GetRoomDetailsAsync(Guid roomId);
        // Potentially a method to check overall room availability if not fully handled by Booking Service
    }

    public class RoomDetailsDto
    {
        public Guid Id { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal BasePricePerNight { get; set; }
        public int Capacity { get; set; }
    }

    public class AddonDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal PricePerUnit { get; set; }
    }

    // src/BookingService.Infrastructure/Integrations/RoomMgmt/RoomManagementServiceClient.cs


    public class RoomManagementService : IRoomManagementService
    {
        private readonly ILogger<RoomManagementService> _logger;
        private readonly IRoomRepository _roomRepository;

        public RoomManagementService(IRoomRepository roomRepository, ILogger<RoomManagementService> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        public async Task<RoomDetailsDto?> GetRoomDetailsAsync(Guid roomId)
        {
            try
            {
                var result = await _roomRepository.GetByIdAsync(roomId);
                return new RoomDetailsDto() { Id = result.Id };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting room details for room {RoomId} from Room Management Service.",
                    roomId);
                // Handle specific status codes if needed (e.g., 404 Not Found)
                return null; // Or throw a custom exception
            }
        }
    }
