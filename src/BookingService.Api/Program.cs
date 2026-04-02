// src/BookingService.Api/Program.cs
using BookingService.Application.Services;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Integrations.RoomMgmt;
//using BookingService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BookingService.Application.Mapping;
using BookingService.Domain;
using BookingService.Infrastructure;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Persistence.Repositories;
//using Serilog;
using Microsoft.Extensions.Options; // For IOptions<T>

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
var configuration = builder.Configuration;

// Configure Serilog
//builder.Host.UseSerilog((context, configuration) =>
  //  configuration.ReadFrom.Configuration(context.Configuration)
   //              .Enrich.FromLogContext()
    //             .WriteTo.Console()); // Log to console

// --- Services ---

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
// Add IUnitOfWork if you have implemented it
// builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Infrastructure: Integrations
// Configure HttpClient for external services
//builder.Services.AddHttpClient<IRoomManagementService, RoomManagementService>(client =>
//{
   /// client.BaseAddress = new Uri(configuration["Services:RoomManagementServiceUrl"]);
         //});

// Infrastructure: Messaging (Example: Mock)
// Replace with actual implementation (e.g., RabbitMQ client)
//builder.Services.AddSingleton<IMessagePublisher, MockMessagePublisher>();

// Application Layer Services
builder.Services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();
builder.Services.AddScoped<IRoomManagementService, RoomManagementService>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(MappingConfiguration)); // Assuming your profile is named MappingConfiguration in Application layer
builder.Services.AddAutoMapper(typeof(MappingConfiguration).Assembly);
// Add other services like Authentication, Authorization, Health Checks etc.

// Option pattern for configuration
// builder.Services.Configure<MyServiceOptions>(configuration.GetSection("MyServiceOptions"));

var app = builder.Build();

// --- Middleware Pipeline ---

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
        try {
             dbContext.Database.Migrate();
             // Seed data here if needed
             // AppDbContextSeed.SeedAsync(dbContext).Wait();
        } catch (Exception ex)
        {
            var kkk = ex;
            //  Log.Error(ex, "Database migration failed during startup.");
            // Decide how to handle this failure - maybe stop the app?
        }
    }
}

app.UseHttpsRedirection();

// Use Serilog Request Logging
//app.UseSerilogRequestLogging();

// Add Authentication and Authorization middleware if implemented
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
