namespace BookingService.Domain.Entities;


public class EmailOutboxMessage
{
    public Guid Id { get; set; }
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public string EmailType { get; set; } = string.Empty; // "Reminder", "Confirmation" etc.
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyTemplateKey { get; set; } = string.Empty;
    public string? TemplateDataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum OutboxMessageStatus
{
    // Ожидает отправки
    Pending = 0,
    // Успешно отправлено
    // После успешной отправки через SMTP
    Sent = 1,
    // Ошибка отправки
    // После неудачной попытки отправки
    Failed = 2
}
