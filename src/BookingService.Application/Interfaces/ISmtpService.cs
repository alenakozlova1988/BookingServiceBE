namespace BookingService.Application.Interfaces;

/// <summary>
/// Сервис для отправки email-сообщений
/// </summary>
public interface ISmtpService
{
    /// <summary>
    /// Отправляет email
    /// </summary>
    /// <param name="to">Email получателя</param>
    /// <param name="subject">Тема письма</param>
    /// <param name="body">HTML или текст письма</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправляет email с вложениями
    /// </summary>
    Task SendWithAttachmentAsync(
        string to,
        string subject,
        string body,
        byte[] attachment,
        string attachmentFileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправляет email нескольким получателям
    /// </summary>
    Task SendBulkAsync(
        IReadOnlyList<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет доступность SMTP-сервера (для health checks)
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
