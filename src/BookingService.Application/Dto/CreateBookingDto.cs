using System;
using System.ComponentModel.DataAnnotations;

namespace BookingService.Application.Dto
{
    public class CreateBookingDto
    {
        /// <summary>
        /// Дата бронирования в формате ISO 8601 (например, "2026-03-31T16:11:19.461Z").
        /// Используем DateTimeOffset для корректной обработки UTC времени.
        /// </summary>
        [Required(ErrorMessage = "Дата является обязательным полем")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Название выбранной переговорной комнаты.
        /// </summary>
        [Required(ErrorMessage = "Необходимо указать название комнаты")]
        [StringLength(100, MinimumLength = 2)]
        public string RoomId { get; set; }

        /// <summary>
        /// Название или заголовок встречи.
        /// </summary>
        [Required(ErrorMessage = "Заголовок встречи обязателен")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Индекс часа начала встречи (например: 0 для 08:00, 1 для 09:00 и т.д.).
        /// </summary>
        [Range(0, 24, ErrorMessage = "Индекс начала часа должен быть в пределах суток")]
        public int StartHourIndex { get; set; }

        /// <summary>
        /// Продолжительность бронирования в часах.
        /// </summary>
        [Range(1, 12, ErrorMessage = "Продолжительность должна быть от 1 до 12 часов")]
        public int Duration { get; set; }

        /// <summary>
        /// Вычисляемое свойство (опционально) для получения индекса часа завершения.
        /// Удобно использовать в бизнес-логике на стороне сервера.
        /// </summary>
        public int EndHourIndex => StartHourIndex + Duration;
    }
}