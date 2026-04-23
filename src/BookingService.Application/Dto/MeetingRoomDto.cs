using System.ComponentModel.DataAnnotations;
using BookingService.Application.Enum;

namespace BookingService.Application.Dto;

public class MeetingRoomDto
{
    // В .NET тип Guid автоматически парсится из строки, приходящей с фронта
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Capacity { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Цвет комнаты. 
    /// </summary>
    [Required]
    public MeetingRoomColor Color { get; set; } = MeetingRoomColor.Teal;
}