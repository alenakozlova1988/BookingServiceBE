using Prometheus;

namespace BookingService.Infrastructure.Services;

public class BookingMetricsService
{
    private static readonly Counter BookingCreatedCounter = Metrics
        .CreateCounter("bookings_created_total", "Общее количество созданных бронирований",
            new CounterConfiguration
            {
                LabelNames = new[] { "room_id", "user_id" }
            });

    private static readonly Counter BookingFailedCounter = Metrics
        .CreateCounter("bookings_failed_total", "Количество неудачных попыток бронирования");

    private static readonly Histogram BookingDurationHistogram = Metrics
        .CreateHistogram("bookings_creation_duration_seconds", "Время создания бронирования",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10), // 0.1, 0.2, 0.4, ... 
                LabelNames = new[] { "room_id" }
            });

    // Gauge для активных бронирований (опционально)
    private static readonly Gauge ActiveBookings = Metrics
        .CreateGauge("bookings_active_count", "Количество активных бронирований");

    public void RecordBookingCreated(string roomId, string userId)
    {
        BookingCreatedCounter.WithLabels(roomId, userId).Inc();
        ActiveBookings.Inc();
    }

    public void RecordBookingFailed()
    {
        BookingFailedCounter.Inc();
    }

    public IDisposable MeasureBookingDuration(string roomId)
    {
        return BookingDurationHistogram.WithLabels(roomId).NewTimer();
    }

    public void RecordBookingCompleted()
    {
        ActiveBookings.Dec();
    }
}
