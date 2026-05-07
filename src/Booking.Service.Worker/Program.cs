using Booking.Service.Worker;
using Booking.Service.Worker.HealthChecks;
using Booking.Service.Worker.Worker;
using Booking.Service.Worker.Worker.Infrastructure;
using BookingService.Application;
using BookingService.Application.Interfaces;
using BookingService.Application.Services;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Persistence.Repositories;
using BookingService.Infrastructure.Services;
using BookingService.Worker.Diagnostics;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources; // Пространство имен для ваших сервисов

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- РЕГИСТРАЦИЯ РЕПОЗИТОРИЕВ И OUTBOX (Обязательно для ReminderJob) ---
builder.Services.AddScoped<IBookingReminderRepository, BookingReminderRepository>();
builder.Services.AddScoped<IEmailOutboxService, EmailOutboxService>(); 
// Регистрируем сервисы
builder.Services.AddSingleton<IEmailTemplateService, FileEmailTemplateService>();
//SMTP-клиент SmtpClient лучше создавать на каждую отправку (он не thread-safe)
// Transient - каждая отправка письма
builder.Services.AddScoped<ISmtpService, SmtpService>(); // Scoped для корректного управления соединением

// 1. Добавляем Producer (он нужен для EmailOutboxPublisher)
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        // Берем адрес из конфига, который у вас уже есть
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
        // Настройки надежности
        Acks = Acks.All,
        EnableIdempotence = true
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// Регистрируем Kafka Consumer (как Singleton)
builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = "localhost:9092",
        GroupId = "email-worker-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("BookingService.Worker", serviceVersion: "1.0.0"))
    .WithMetrics(metrics => metrics
        .AddMeter("BookingService.Worker")
        .AddPrometheusExporter(options => 
        {
            options.ScrapeEndpointPath = "/metrics";
            options.ScrapeResponseCacheDurationMilliseconds = 0;
        }));


builder.Services.AddWorkerMetrics();

// Наши метрики
builder.Services.AddWorkerMetrics();

// Регистрируем Worker
builder.Services.AddHostedService<EmailConsumerWorker>();
builder.Services.AddHostedService<ReminderJob>();
builder.Services.AddScoped<IEmailOutboxPublisher, EmailOutboxPublisher>(); // Это у вас уже есть
builder.Services.AddHostedService<EmailOutboxWorker>(); // ДОБАВЬТЕ ЭТО



// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<SmtpHealthCheck>("smtp");

var host = builder.Build();
host.Run();