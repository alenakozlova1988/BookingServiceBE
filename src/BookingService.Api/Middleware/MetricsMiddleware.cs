using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BookingService.Api.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Counter<int> _requestsCounter;
    private readonly Counter<int> _errorCounter;
    private readonly Histogram<double> _requestDuration;

    public MetricsMiddleware(RequestDelegate next, IMeterFactory meterFactory)
    {
        _next = next;
        var meter = meterFactory.Create("BookingService.Api");

        // RPS - Request per second (через Rate в Prometheus)
        _requestsCounter = meter.CreateCounter<int>(
            "booking_requests_total",
            description: "Total number of requests");

        // Ошибки 5xx
        _errorCounter = meter.CreateCounter<int>(
            "booking_errors_total",
            description: "Total number of 5xx errors");

        // Latency (гистограмма)
        _requestDuration = meter.CreateHistogram<double>(
            "booking_request_duration_seconds",
            unit: "seconds",
            description: "Request duration in seconds");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        // Добавляем теги для детализации
        var tags = new TagList
        {
            { "endpoint", path },
            { "method", method }
        };

        _requestsCounter.Add(1, tags);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            _errorCounter.Add(1, tags);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _requestDuration.Record(stopwatch.Elapsed.TotalSeconds, tags);

            // Если статус код 5xx - считаем ошибкой
            if (context.Response.StatusCode >= 500)
            {
                _errorCounter.Add(1, tags);
            }
        }
    }
}

// Extension method
public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseMetricsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
}
