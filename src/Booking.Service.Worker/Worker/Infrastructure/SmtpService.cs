using System.Diagnostics;
using System.Net.Mail;
using BookingService.Application.Interfaces;
using Prometheus;

namespace Booking.Service.Worker.Worker.Infrastructure;

public class SmtpService : ISmtpService
{
    private readonly SmtpClient _client;
    private readonly ILogger<SmtpService> _logger;
    private readonly Counter _emailSentCounter = Metrics
        .CreateCounter("emails_sent_total", "Количество отправленных писем");

    public SmtpService(ILogger<SmtpService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _client = new SmtpClient
        {
            Host = configuration["Smtp:Host"] ?? "localhost",
            Port = int.Parse(configuration["Smtp:Port"] ?? "1025"), // 1025 для MailHog
            EnableSsl = bool.Parse(configuration["Smtp:EnableSsl"] ?? "false"),
            Credentials = null, // Для локального теста без авторизации
            // Timeout = 5000
        };
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await _client.SendMailAsync(new MailMessage("noreply@booking.com", to, subject, body));
            _emailSentCounter.Inc();
            _logger.LogInformation("Email sent to {To} in {Elapsed}ms", to, sw.ElapsedMilliseconds);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP failed for {To}", to);
            throw; // Polly перехватит
        }
        finally
        {
            // Записать метрику времени отправки
        }
    }
    

    public Task SendWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string attachmentFileName,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SendBulkAsync(IReadOnlyList<string> recipients, string subject, string body,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}