using System.Text.Json;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Application.Services;

public class EmailOutboxService : IEmailOutboxService
{
    private readonly AppDbContext _context;

    public EmailOutboxService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BookingEmailEvent emailEvent, Guid? relatedBookingId = null)
    {
        var outbox = new EmailOutboxMessage()
        {
            Id = Guid.NewGuid(),
            EmailType = emailEvent.EmailType,
            To = emailEvent.To,
            Subject = emailEvent.Subject,
            BodyTemplateKey = emailEvent.BodyTemplateKey,
            TemplateDataJson = JsonSerializer.Serialize(emailEvent.TemplateData),
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsSent = false,
            RetryCount = 0
        };

        await _context.EmailOutbox.AddAsync(outbox);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<EmailOutboxMessage>> GetUnsentEmailsAsync(int batchSize = 10)
    {
        return await _context.EmailOutbox
            .Where(e => !e.IsSent && e.RetryCount < 3)
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkSentAsync(Guid id)
    {
        var email = await _context.EmailOutbox.FindAsync(id);
        if (email != null)
        {
            email.IsSent = true;
            email.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementRetryCountAsync(Guid id, string errorMessage)
    {
        var email = await _context.EmailOutbox.FindAsync(id);
        if (email != null)
        {
            email.RetryCount++;
            email.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
    }
    

    public Task<IReadOnlyList<EmailOutboxMessage>> GetPendingAsync(int batchSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task MarkFailedAsync(Guid messageId, string errorMessage)
    {
        throw new NotImplementedException();
    }
}