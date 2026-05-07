// Booking.Service.Worker/HealthChecks/SmtpHealthCheck.cs

using BookingService.Application.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Booking.Service.Worker.HealthChecks
{
    public class SmtpHealthCheck : IHealthCheck
    {
        private readonly ISmtpService _smtpService;
        private readonly ILogger<SmtpHealthCheck> _logger;

        public SmtpHealthCheck(ISmtpService smtpService, ILogger<SmtpHealthCheck> logger)
        {
            _smtpService = smtpService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Отправляем тестовое письмо самому себе
                // В production лучше использовать отдельный SMTP-команду NOOP или проверку соединения
                await _smtpService.SendAsync(
                    to: "health@booking.com",
                    subject: "Health Check",
                    body: "This is a health check email - please ignore."
                );

                _logger.LogInformation("SMTP health check passed");

                return HealthCheckResult.Healthy("SMTP is reachable and functioning");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP health check failed");

                return HealthCheckResult.Unhealthy(
                    description: "SMTP is not reachable",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        ["LastErrorTime"] = DateTime.UtcNow,
                        ["ErrorType"] = ex.GetType().Name
                    }
                );
            }
        }
    }
}