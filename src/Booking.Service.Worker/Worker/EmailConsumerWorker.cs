using System.Text.Json;
using BookingService.Application.Interfaces;
using Confluent.Kafka;
using Polly;
using Polly.Retry;

namespace Booking.Service.Worker.Worker;
public class EmailConsumerWorker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory; // Используем фабрику
    private readonly IEmailTemplateService _templates;     // Это Singleton, можно оставить
    private readonly ILogger<EmailConsumerWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public EmailConsumerWorker(
        IConsumer<string, string> consumer, 
        IServiceScopeFactory scopeFactory, // Внедряем фабрику
        IEmailTemplateService templates,
        ILogger<EmailConsumerWorker> logger)
    {
        _consumer = consumer;
        _scopeFactory = scopeFactory;
        _templates = templates;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("booking.emails");

        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            {
                var cr = _consumer.Consume(stoppingToken); // Это блокирующий вызов
                if (cr == null) continue;

                // СОЗДАЕМ SCOPE на каждое сообщение
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Достаем Scoped/Transient сервисы из Scope
                    var smtp = scope.ServiceProvider.GetRequiredService<ISmtpService>();

                    var @event = JsonSerializer.Deserialize<BookingEmailEvent>(cr.Message.Value);

                    // Передаем smtp сервис в метод обработки
                    await _retryPolicy.ExecuteAsync(() => ProcessEvent(@event, smtp));
                }

                _consumer.Commit(cr);
            }
            catch (Exception ex) { /* log */ }
        }
    }

   // private async Task ProcessEvent(BookingEmailEvent ev, ISmtpService smtp)
  //  {
  //      var body = await _templates.RenderAsync(ev.BodyTemplateKey, ev.TemplateData);

        // ... логика с задержкой ...

   //     await smtp.SendAsync(ev.To, ev.Subject, body);
  //  }
    
    private async Task ProcessEvent(BookingEmailEvent ev, ISmtpService smtp)
    {
        // Преобразуем Dictionary из Kafka обратно в строгую модель для Razor
        var templateModel = JsonSerializer.Deserialize<EmailTemplateModel>(
            JsonSerializer.Serialize(ev.TemplateData) 
        );
        

        // Рендерим шаблон
       //var body = await _templates.RenderAsync(ev.BodyTemplateKey, templateModel);
        var body = await _templates.RenderTemplateAsync(ev.BodyTemplateKey, templateModel);
        await smtp.SendAsync(ev.To, ev.Subject, body);
    }
}