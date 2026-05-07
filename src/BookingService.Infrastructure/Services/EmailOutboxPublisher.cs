using System.Text.Json;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;


namespace BookingService.Infrastructure.Services;

// Booking.Service.Infrastructure/Services/EmailOutboxPublisher.cs
public class EmailOutboxPublisher : IEmailOutboxPublisher
{
    private readonly AppDbContext _db;
    private readonly IProducer<string, string> _kafkaProducer;

    public EmailOutboxPublisher(AppDbContext db, IProducer<string, string> kafkaProducer)
    {
        _db = db;
        _kafkaProducer = kafkaProducer;
    }

    public async Task PublishPendingAsync()
    {
        var pending = await _db.EmailOutbox
            .Where(o => o.Status == OutboxMessageStatus.Pending)
            .Take(50)
            .ToListAsync();

        foreach (var msg in pending)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(msg.TemplateDataJson ?? "{}");
                if (data == null)
                    return;

                var kafkaMessage = new BookingEmailEvent
                {
                    To = msg.To, // Убедитесь, что эти поля есть в сущности Outbox
                    Subject = msg.Subject, // Если их нет, добавьте в таблицу Outbox
                    BodyTemplateKey = msg.BodyTemplateKey, // Например, "BookingConfirmation"
                    TemplateData = data
                };

                 await _kafkaProducer.ProduceAsync(
                    "booking.emails",
                    new Message<string, string>
                    {
                        Key = msg.Id.ToString(),
                        Value = JsonSerializer.Serialize(kafkaMessage), // Сериализуем ВЕСЬ объект
                    });

                msg.Status = OutboxMessageStatus.Sent;
                msg.SentAt = DateTime.UtcNow;
                msg.IsSent = true;
            }
            catch (Exception ex)
            {
                msg.ErrorMessage = ex.Message;
                msg.Status = OutboxMessageStatus.Failed;
                msg.RetryCount++;
                // Retry logic в БД позже
            }
        }

        await _db.SaveChangesAsync();
    }
}
