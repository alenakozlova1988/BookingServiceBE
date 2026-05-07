using BookingService.Application.Interfaces;

namespace Booking.Service.Worker;

public class EmailOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailOutboxWorker> _logger;

    public EmailOutboxWorker(IServiceScopeFactory scopeFactory, ILogger<EmailOutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Поскольку EmailOutboxPublisher — Scoped, создаем Scope
                using (var scope = _scopeFactory.CreateScope())
                {
                    var publisher = scope.ServiceProvider.GetRequiredService<IEmailOutboxPublisher>();

                    // ВАЖНО: вызываем тот самый метод
                    await publisher.PublishPendingAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при публикации сообщений из Outbox");
            }

            // Ждем паузу (например, 10 секунд), чтобы не бомбить базу слишком часто
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}