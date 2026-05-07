public class BookingEmailEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EmailType { get; set; } // "BookingConfirmation", "Reminder"
    public string To { get; set; }
    public string Subject { get; set; }
    public string BodyTemplateKey { get; set; } // имя файла-шаблона
    public Dictionary<string, string> TemplateData { get; set; }
    public DateTime ScheduledAtUtc { get; set; } // для напоминаний
}