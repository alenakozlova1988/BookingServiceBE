using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BookingService.Worker.Diagnostics;

public class WorkerMetrics
{
    private readonly Counter<int> _processedMessagesCounter;
    private readonly Counter<int> _failedMessagesCounter;
    private readonly Histogram<double> _processingDuration;
    private readonly Counter<int> _emailsSentCounter;
    private readonly Counter<int> _outboxEventsProcessed;

    public WorkerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("BookingService.Worker");

        _processedMessagesCounter = meter.CreateCounter<int>(
            "worker_messages_processed_total",
            description: "Total number of processed messages");

        _failedMessagesCounter = meter.CreateCounter<int>(
            "worker_messages_failed_total",
            description: "Total number of failed messages");

        _processingDuration = meter.CreateHistogram<double>(
            "worker_message_processing_duration_seconds",
            unit: "seconds",
            description: "Message processing duration");

        _emailsSentCounter = meter.CreateCounter<int>(
            "worker_emails_sent_total",
            description: "Total number of sent emails");

        _outboxEventsProcessed = meter.CreateCounter<int>(
            "worker_outbox_events_processed_total",
            description: "Total number of outbox events processed");
    }

    public void RecordMessageProcessed(string messageType)
    {
        _processedMessagesCounter.Add(1, new TagList { { "message_type", messageType } });
    }

    public void RecordMessageFailed(string messageType, string errorType)
    {
        _failedMessagesCounter.Add(1, new TagList 
        { 
            { "message_type", messageType },
            { "error_type", errorType }
        });
    }

    public void RecordProcessingDuration(double durationSeconds, string messageType)
    {
        _processingDuration.Record(durationSeconds, new TagList { { "message_type", messageType } });
    }

    public void RecordEmailSent(string emailType)
    {
        _emailsSentCounter.Add(1, new TagList { { "email_type", emailType } });
    }

    public void RecordOutboxEventProcessed(string eventType, bool success)
    {
        _outboxEventsProcessed.Add(1, new TagList 
        { 
            { "event_type", eventType },
            { "success", success.ToString() }
        });
    }
}

// Регистрация в DI
public static class WorkerMetricsExtensions
{
    public static IServiceCollection AddWorkerMetrics(this IServiceCollection services)
    {
        services.AddSingleton<WorkerMetrics>();
        return services;
    }
}
