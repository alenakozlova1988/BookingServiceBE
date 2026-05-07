// src/BookingService.Api/Program.cs
using BookingService.Application.Services;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Integrations.RoomMgmt;
//using BookingService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

using BookingService.Api.Middleware;
using BookingService.Application.Interfaces;
using BookingService.Application.Mapping;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Middlewares;
using BookingService.Infrastructure.Persistence.Repositories;
using BookingService.Infrastructure.Services;
//using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Prometheus;
using Serilog; // For IOptions<T>

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));
// --- Configuration ---
var configuration = builder.Configuration;

// 1. Определяем имя политики
    var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 2. Добавляем сервис CORS в контейнер
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: myAllowSpecificOrigins,
            policy =>
            {
                policy.WithOrigins("http://localhost:4200") // URL вашего Angular/Frontend приложения
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });


// Add controllers
builder.Services.AddControllers();

// Add API Explorer for Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo { Title = "Booking Service API", Version = "v1" });
});

// EF Core DbContext
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)); // Use Npgsql for PostgreSQL

// Infrastructure: Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMeetingRoomService, MeetingRoomService>();

// Add IUnitOfWork if you have implemented it
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure: Messaging (Example: Mock)
// Replace with actual implementation (e.g., RabbitMQ client)
//builder.Services.AddSingleton<IMessagePublisher, MockMessagePublisher>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IKratosService, KratosService>();
// Application Layer Services
builder.Services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();
builder.Services.AddScoped<IRoomManagementService, RoomManagementService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IGravatarService, GravatarService>();
builder.Services.AddSingleton<BookingMetricsService>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(MappingConfiguration)); // Assuming your profile is named MappingConfiguration in Application layer
builder.Services.AddAutoMapper(typeof(MappingConfiguration).Assembly);

// Add other services like Authentication, Authorization, Health Checks etc.
builder.Services.AddAuthorization();

// Prometheus метрики
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("BookingService.Api", serviceVersion: "1.0.0"))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()  // Автоматические метрики ASP.NET
        .AddHttpClientInstrumentation()  // Для внешних вызовов
        .AddMeter("BookingService.*")    // Наши кастомные метрики
        .AddPrometheusExporter());       // Экспорт в Prometheus

var app = builder.Build();

app.UseMiddleware<KratosSessionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking Service API v1"));
    // Seed database or run migrations automatically in development (Use with caution in prod)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        dbContext.Database.EnsureCreated(); // Creates DB if not exists (no migrations)
        // OR Apply migrations:
        try
        {
            dbContext.Database.Migrate();
            // Seed data here if needed
            // AppDbContextSeed.SeedAsync(dbContext).Wait();
        }
        catch (Exception ex)
        {
              Log.Error(ex, "Database migration failed during startup. Error: {0}", ex.Message);
        }
    }
}

app.UseHttpsRedirection();

// 3. Подключаем CORS в конвейер обработки запросов. 
// Важно: UseCors должен стоять ПЕРЕД UseAuthorization и ПЕРЕД MapControllers
app.UseCors(myAllowSpecificOrigins);

app.UseMetricServer(); 
app.UseHttpMetrics();
app.MapMetrics();

// Кастомные метрики
var requestCounter = Metrics.CreateCounter("app_requests_total", "Total requests");
var activeRequests = Metrics.CreateGauge("app_requests_active", "Active requests");
app.MapGet("/", () => {
    Log.Information("HELLO ELK! My test message"); // Тестовый лог
    return "Hello World";
});

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseMetricsMiddleware();

// Add Authentication and Authorization middleware if implemented
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
