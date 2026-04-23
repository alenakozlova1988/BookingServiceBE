using BookingService.Application.Dto;
using BookingService.Application.Services;
using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MeetingRoomBooking.Controllers
{
    // Атрибут [ApiController] включает автоматическую проверку валидности моделей и привязку данных
    // Атрибут [Route] задает базовый путь к API: "api/rooms"
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMeetingRoomService _meetingRoomService;

        // Внедрение контекста базы данных через конструктор
        public RoomsController(AppDbContext context, IMeetingRoomService meetingRoomService)
        {
            _context = context;
            _meetingRoomService = meetingRoomService;
        }

        /// <summary>
        /// Получение списка всех доступных переговорных комнат.
        /// GET: api/rooms
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingRoomDto>>> GetRooms()
        {
            try
            {
                // Получаем все комнаты из базы данных асинхронно
                var rooms = await _meetingRoomService.GetAllMeetingRoomsAsync();

                return Ok(rooms); // Возвращаем статус 200 OK и список данных
            }
            catch (Exception ex)
            {
                // В реальном приложении здесь должен быть логгер (ILogger)
                return StatusCode(500, "Внутренняя ошибка сервера при получении списка комнат");
            }
        }

        /// <summary>
        /// Получение детальной информации о конкретной комнате.
        /// GET: api/rooms/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MeetingRoom>> GetRoom(Guid id)
        {
            // Ищем комнату по первичному ключу
            var room = await _context.MeetingRooms.FindAsync(id);

            if (room == null)
            {
                // Если комната не найдена, возвращаем 404 Not Found
                return NotFound(new { message = $"Переговорная комната с ID {id} не найдена" });
            }

            return Ok(room);
        }

        /// <summary>
        /// Дополнительный метод для фильтрации комнат по статусу (например, только свободные)
        /// GET: api/rooms/available
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<MeetingRoom>>> GetAvailableRooms()
        {
            var availableRooms = await _context.MeetingRooms
                .Where(x => x.Status == Status.Available)
                .ToListAsync();

            return Ok(availableRooms);
        }
    }
}
