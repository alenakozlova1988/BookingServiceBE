using BookingService.Domain.Entities;

namespace BookingService.Application.Interfaces;

/// <summary>
/// Сервис для работы с Outbox-паттерном для email-уведомлений
/// </summary>
public interface IEmailOutboxService
{
    /// <summary>
    /// Сохраняет письмо для последующей фоновой отправки
    /// </summary>
    Task AddAsync(BookingEmailEvent ev,
        Guid? relatedBookingId = null);

    /// <summary>
    /// Возвращает неотправленные письма (для обработки воркером)
    /// </summary>
    Task<IReadOnlyList<EmailOutboxMessage>> GetPendingAsync(int batchSize = 10);

    /// <summary>
    /// Помечает письмо как отправленное
    /// </summary>
    Task MarkSentAsync(Guid messageId);

    /// <summary>
    /// Помечает письмо как ошибочное (после нескольких неудачных попыток)
    /// </summary>
    Task MarkFailedAsync(Guid messageId, string errorMessage);
}


