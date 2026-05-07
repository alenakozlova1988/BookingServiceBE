using BookingService.Application.Interfaces;
using BookingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingService.Application;

public class ReminderJob(IServiceScopeFactory scopeFactory, ILogger<ReminderJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Создаем Scope, чтобы получить Scoped сервисы (DB, Outbox)
                using (var scope = scopeFactory.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IBookingReminderRepository>();
                    var bookings = await repository.GetTodayBookingsAsync();
                    logger.LogInformation("Found {Count} bookings for today", bookings.Count);

                    var outbox = scope.ServiceProvider.GetRequiredService<IEmailOutboxService>();
                    // TODO добавить в базу такой параметр
                    // && !b.ReminderSent)
                    if (bookings.Any())
                    {
                        logger.LogInformation("Found {Count} upcoming bookings. Adding to outbox...", bookings.Count);

                        foreach (var booking in bookings)
                        {
                            var ev = new BookingEmailEvent
                            {
                                EmailType = "Reminder",
                                To = booking.User.Email, // Используйте правильное поле из вашей сущности
                                Subject = "Напоминание о бронировании",
                                BodyTemplateKey = "BookingConfirmation",
                                TemplateData = new Dictionary<string, string>
                                {
                                    ["EventName"] = booking.Title,
                                    ["RoomName"] = booking?.MeetingRoom?.Description,
                                    ["Date"] = booking.CheckInDate.ToString("f"),
                                    ["To"] = booking.User.Email
                                }
                            };

                            await outbox.AddAsync(ev);
                        }

                        // Сохраняем все изменения (и брони, и новые записи в аутбоксе) одним разом
                        // Если в репозитории нет SaveChanges, его нужно вызвать через UnitOfWork или добавить в репозиторий
                        await repository.SaveChangesAsync(ct);

                        logger.LogInformation("Successfully added {Count} reminders to outbox", bookings.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing reminders");
            }

            // Ждем перед следующей проверкой
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}